namespace EasyBuy.Domain.Primitives;

public abstract class BaseEntity<TId>
{
    protected BaseEntity()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
    }

    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

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

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public override string ToString()
    {
        return $"{GetType().Name} [CreatedAt={CreatedAt}, UpdatedAt={UpdatedAt}, IsDeleted={IsDeleted}]";
    }
}