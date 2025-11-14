namespace EasyBuy.Application.DTOs.Reviews;

/// <summary>
/// Data transfer object for Review entity.
/// Includes user and product information.
/// </summary>
public sealed class ReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime ReviewDate { get; set; }
    public int HelpfulCount { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
