namespace EasyBuy.Domain.Primitives;

public abstract class Entity<TId>
{
    /*
     *Sorumluluk: Bir varlığın temel özelliklerini tanımlar. Örneğin, sadece bir Id'ye sahiptir ve diğer özellikleri barındırmaz.
       Kullanım Alanı: Domain'e özgü tüm varlıkların temel soyut sınıfıdır.
     */

    protected Entity()
    {
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected set; }

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var entity = (Entity<TId>)obj;
        return EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}