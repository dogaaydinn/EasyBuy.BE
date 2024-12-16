using System.ComponentModel.DataAnnotations;

namespace EasyBuy.Domain.Primitives;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsDeleted { get; set; }

    public virtual void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdateTimestamp();
    }

    public virtual void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        UpdateTimestamp();
    }

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public override string ToString()
    {
        return $"{GetType().Name} [CreatedAt={CreatedAt}, UpdatedAt={UpdatedAt}, IsDeleted={IsDeleted}]";
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}