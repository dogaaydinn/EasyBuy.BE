namespace EasyBuy.Domain.Primitives;

public abstract class BaseEntity<TId> : Entity<TId>
{
    /*
 Sorumluluk: Domain içinde ortak davranışları ve özellikleri tanımlar (ör. CreatedAt, UpdatedAt, IsDeleted).
   Kullanım Alanı: Daha genel bir yapı sağlar ve Entity'den türetilir.
 */

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public bool IsDeleted { get; private set; }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}