using EasyBuy.Domain.Primitives;

namespace EasyBuy.Domain.Entities;

public class File : BaseEntity
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string? Extension { get; set; }
    public long Size { get; set; }
    public required string ContentType { get; set; }
}