using EasyBuy.Application.Common.Models;
using EasyBuy.Application.Features.Baskets.DTOs;
using MediatR;

namespace EasyBuy.Application.Features.Baskets.Queries.GetBasket;

public sealed record GetBasketQuery : IRequest<Result<BasketDto>>;
