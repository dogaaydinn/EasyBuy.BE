using Asp.Versioning;
using EasyBuy.Application.Features.Categories.Commands;
using EasyBuy.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyBuy.WebAPI.Controllers.v1;

/// <summary>
/// Categories management API endpoints.
/// Provides CRUD operations for hierarchical categories with CQRS pattern.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories with pagination and filtering.
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>Paged list of categories</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategories([FromQuery] GetCategoriesQuery query)
    {
        _logger.LogInformation("Getting categories: Page={Page}, Size={Size}, ParentId={ParentId}", 
            query.PageNumber, query.PageSize, query.ParentCategoryId);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get hierarchical category tree.
    /// Returns all categories in tree structure with nested subcategories.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories</param>
    /// <returns>Hierarchical category tree</returns>
    [HttpGet("tree")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategoryTree([FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting category tree, IncludeInactive={IncludeInactive}", includeInactive);

        var query = new GetCategoryTreeQuery { IncludeInactive = includeInactive };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get category by ID.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        _logger.LogInformation("Getting category: {CategoryId}", id);

        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new category.
    /// </summary>
    /// <param name="command">Category creation data</param>
    /// <returns>Created category ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        _logger.LogInformation("Creating category: {CategoryName}, Parent: {ParentId}", 
            command.Name, command.ParentCategoryId);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetCategoryById),
            new { id = result.Data },
            result);
    }

    /// <summary>
    /// Update an existing category.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Category update data</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        _logger.LogInformation("Updating category: {CategoryId}", id);

        command.CategoryId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a category.
    /// Soft delete if category has products or subcategories, hard delete otherwise.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="forceDelete">Force hard delete (fails if category has products/subcategories)</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory(Guid id, [FromQuery] bool forceDelete = false)
    {
        _logger.LogInformation("Deleting category: {CategoryId}, ForceDelete: {ForceDelete}", id, forceDelete);

        var command = new DeleteCategoryCommand 
        { 
            CategoryId = id, 
            ForceDelete = forceDelete 
        };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
