using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Users;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace EasyBuy.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly EasyBuyDbContext _context;

    public LoginCommandHandler(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        EasyBuyDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email or username
        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername)
                   ?? await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid email/username or password");
        }

        // Check if user is deleted
        if (user.IsDeleted)
        {
            return Result<AuthResponseDto>.Failure("This account has been deactivated");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Result<AuthResponseDto>.Failure("Account is locked out. Please try again later.");
            }

            return Result<AuthResponseDto>.Failure("Invalid email/username or password");
        }

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.UserName!, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Extract JTI from access token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);
        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? Guid.NewGuid().ToString();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Token = refreshToken,
            JwtId = jti,
            IsUsed = false,
            IsRevoked = false,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        await _context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
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

        return Result<AuthResponseDto>.Success(response, "Login successful");
    }
}
