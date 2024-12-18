using System.Net;
using EasyBuy.Application.Abstractions.Storage;
using EasyBuy.Application.Repositories.File;
using EasyBuy.Application.Repositories.Product;
using EasyBuy.Application.ViewModels.Products;
using EasyBuy.Domain.Entities;
using EasyBuy.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IProductImageFileWriteRepository _productImageFileWriteRepository;
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IStorageService _storageService;

    public ProductsController(IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository,
        IWebHostEnvironment hostingEnvironment,
        IProductImageFileWriteRepository productImageFileWriteRepository,
        IStorageService storageService)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _hostingEnvironment = hostingEnvironment;
        _productImageFileWriteRepository = productImageFileWriteRepository;
        _storageService = storageService;
    }

    [HttpGet]
    public ObjectResult Get()
    {
        var entities = _productReadRepository.GetAll();

        return Ok(entities);
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
    public async Task<IActionResult> UploadImage(IFormFileCollection files)
    {
        var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

        await _storageService.UploadFilesAsync("mydoga", files);
        return Ok();
    }
}