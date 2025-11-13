using EasyBuy.Application.Common.Interfaces;
using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Users;
using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Domain.Events;
using EasyBuy.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace EasyBuy.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IPublisher _publisher;
    private readonly IEmailService _emailService;
    private readonly EasyBuyDbContext _context;

    public RegisterCommandHandler(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IPublisher publisher,
        IEmailService emailService,
        EasyBuyDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _publisher = publisher;
        _emailService = emailService;
        _context = context;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<AuthResponseDto>.Failure("User with this email already exists");
        }

        var existingUserName = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserName != null)
        {
            return Result<AuthResponseDto>.Failure("Username is already taken");
        }

        // Create new user
        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true // Set to false if you want email confirmation
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure($"Failed to create user: {errors}");
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "Customer");

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

        // Publish domain event
        await _publisher.Publish(new UserRegisteredEvent(user.Id, user.Email!, user.FullName), cancellationToken);

        // Send welcome email (fire and forget)
        _ = _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!, cancellationToken);

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

        return Result<AuthResponseDto>.Success(response, "Registration successful");
    }
}
