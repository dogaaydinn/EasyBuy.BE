namespace EasyBuy.Application.DTOs.Reviews;

/// <summary>
/// Data transfer object for creating a new review.
/// </summary>
public sealed class CreateReviewDto
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
