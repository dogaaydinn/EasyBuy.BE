namespace EasyBuy.Domain.Primitives;

public abstract class BaseEntity<TId>
{
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; private set; } = false;

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdateTimestamp();
    }

    public void Restore()
    {
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
}