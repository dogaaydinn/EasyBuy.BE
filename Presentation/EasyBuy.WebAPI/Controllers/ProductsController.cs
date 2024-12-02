using EasyBuy.Application.Product;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    

    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;

    public ProductsController(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
    }

    [HttpGet]
    public async Task Get()
    {
        var entities = await _productWriteRepository.AddRangeAsync(
        [
            new Product
            {
                Name = "Product 1",
                Price = 100,
                ProductType = ProductType.Electronics,
                Description = null
            }
        ]);

        await _productWriteRepository.SaveChangesAsync();
    }
}