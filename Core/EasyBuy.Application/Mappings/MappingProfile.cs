using AutoMapper;
using EasyBuy.Application.DTOs.Baskets;
using EasyBuy.Application.DTOs.Orders;
using EasyBuy.Application.DTOs.Products;
using EasyBuy.Application.DTOs.Users;
using EasyBuy.Application.Features.Products.Commands.CreateProduct;
using EasyBuy.Application.Features.Products.Commands.UpdateProduct;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Entities.Identity;

namespace EasyBuy.Application.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.ProductImageFiles.Select(x => x.Path).ToList()))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Stock > 0));

        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ProductImageFiles.FirstOrDefault() != null ? src.ProductImageFiles.FirstOrDefault()!.Path : null))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Stock > 0));

        CreateMap<Product, ProductDetailDto>()
            .IncludeBase<Product, ProductDto>();

        CreateMap<CreateProductDto, Product>();
        CreateMap<CreateProductCommand, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName ?? "Anonymous"));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
            .ForMember(dest => dest.Delivery, opt => opt.Ignore()) // TODO: Add when Delivery entity is created
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payments.FirstOrDefault()));

        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Count));

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<Delivery, DeliveryDto>();

        CreateMap<Payment, PaymentDto>();

        // Basket mappings
        CreateMap<Basket, BasketDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.BasketItems));

        CreateMap<BasketItem, BasketItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ProductImageFiles.FirstOrDefault() != null ? src.Product.ProductImageFiles.FirstOrDefault()!.Path : null))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Product.Stock > 0));

        // User/Identity mappings
        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<AppUser, UserProfileDto>()
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSpent, opt => opt.Ignore());

        CreateMap<RegisterUserDto, AppUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>();
    }
}
