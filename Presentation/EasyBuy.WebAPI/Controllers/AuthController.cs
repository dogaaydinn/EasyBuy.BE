using Asp.Versioning;
using EasyBuy.Application.Features.Auth.Commands.Login;
using EasyBuy.Application.Features.Auth.Commands.RefreshToken;
using EasyBuy.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

/// <summary>
/// Authentication endpoints for user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="command">Registration details including email, username, and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens and user details</returns>
    /// <response code="200">User successfully registered</response>
    /// <response code="400">Invalid registration data or user already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", command.Email);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Errors}", command.Email, string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogInformation("User registered successfully: {Email}", command.Email);
        return Ok(result);
    }

    /// <summary>
    /// Authenticate user and obtain JWT tokens
    /// </summary>
    /// <param name="command">Login credentials (email/username and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT tokens and user details</returns>
    /// <response code="200">Successfully authenticated</response>
    /// <response code="400">Invalid credentials or account locked</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for: {EmailOrUsername}", command.EmailOrUsername);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for: {EmailOrUsername}. Reason: {Errors}", command.EmailOrUsername, string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogInformation("User logged in successfully: {EmailOrUsername}", command.EmailOrUsername);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using a valid refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication response with fresh JWT tokens</returns>
    /// <response code="200">Token successfully refreshed</response>
    /// <response code="400">Invalid or expired refresh token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh attempt");

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Token refresh failed. Reason: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }

        _logger.LogInformation("Token refreshed successfully");
        return Ok(result);
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details</returns>
    /// <response code="200">User information retrieved</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            UserName = userName,
            Roles = roles
        });
    }
}
