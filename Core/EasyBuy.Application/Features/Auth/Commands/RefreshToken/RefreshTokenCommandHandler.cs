using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Users;
using EasyBuy.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IApplicationDbContext _context;

    public RefreshTokenCommandHandler(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate refresh token
        var jwtId = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (string.IsNullOrEmpty(jwtId))
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token");
        }

        // Find refresh token in database
        var storedRefreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedRefreshToken == null)
        {
            return Result<AuthResponseDto>.Failure("Refresh token not found");
        }

        // Validate refresh token
        if (storedRefreshToken.IsUsed)
        {
            return Result<AuthResponseDto>.Failure("Refresh token has already been used");
        }

        if (storedRefreshToken.IsRevoked)
        {
            return Result<AuthResponseDto>.Failure("Refresh token has been revoked");
        }

        if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure("Refresh token has expired");
        }

        if (storedRefreshToken.JwtId != jwtId)
        {
            return Result<AuthResponseDto>.Failure("Refresh token does not match JWT");
        }

        // Mark refresh token as used
        storedRefreshToken.IsUsed = true;
        await _context.SaveChangesAsync(cancellationToken);

        // Get user
        var user = storedRefreshToken.User;
        if (user == null || user.IsDeleted)
        {
            return Result<AuthResponseDto>.Failure("User not found or has been deactivated");
        }

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.UserName!, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Store new refresh token
        var refreshTokenEntity = new Domain.Entities.Identity.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = newRefreshToken,
            JwtId = Guid.NewGuid().ToString(),
            IsUsed = false,
            IsRevoked = false,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiry = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = user.CreatedDate,
                Roles = roles.ToList()
            }
        };

        return Result<AuthResponseDto>.Success(response, "Token refreshed successfully");
    }
}
