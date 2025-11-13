using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Users;
using MediatR;

namespace EasyBuy.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public required string EmailOrUsername { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; }
}
