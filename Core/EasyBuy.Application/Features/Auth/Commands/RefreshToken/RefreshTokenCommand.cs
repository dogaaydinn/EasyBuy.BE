using EasyBuy.Application.Common.Models;
using EasyBuy.Application.DTOs.Users;
using MediatR;

namespace EasyBuy.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public required string RefreshToken { get; set; }
}
