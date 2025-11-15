using EasyBuy.Application.Common.Models;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Commands.ClearBasket;

public sealed record ClearBasketCommand : IRequest<Result>;
