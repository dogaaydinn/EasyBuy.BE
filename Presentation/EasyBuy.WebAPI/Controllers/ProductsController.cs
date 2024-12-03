using System.Net;
using EasyBuy.Application.Product;
using EasyBuy.Application.ViewModels.Products;
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
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ProductsController(IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository,
        IWebHostEnvironment hostingEnvironment)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _hostingEnvironment = hostingEnvironment;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var entity = await _productReadRepository.GetByIdAsync(id, false);

        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Post(VmCreateProduct product)
    {
        await _productWriteRepository.AddAsync(new Product
        {
            Name = product.Name,
            Price = product.Price,
            ProductType = ProductType.Accessories
        });

        await _productWriteRepository.SaveChangesAsync();
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpPut]
    public async Task<IActionResult> Put(VmUpdateProduct product)
    {
        var entity = await _productReadRepository.GetByIdAsync(product.Id, false);

        entity.Name = product.Name;
        entity.Price = product.Price;

        await _productWriteRepository.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var entity = await _productReadRepository.GetByIdAsync(id, false);

        _productWriteRepository.HardDelete(entity);
        await _productWriteRepository.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> UploadImage()
    {
        var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        Random r = new();

        foreach (var file in Request.Form.Files)
        {
            var fileName = Path.Combine(uploadPath, $"{r.NextDouble()}{Path.GetExtension(file.FileName)}");

            await using FileStream fileStream = new(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: false);

            await file.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }
        return Ok();
    }
}