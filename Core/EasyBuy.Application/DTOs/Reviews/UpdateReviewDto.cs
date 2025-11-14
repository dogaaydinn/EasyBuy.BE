namespace EasyBuy.Application.DTOs.Reviews;

/// <summary>
/// Data transfer object for updating an existing review.
/// </summary>
public sealed class UpdateReviewDto
{
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
