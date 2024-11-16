namespace EasyBuy.Domain.Primitives;

public abstract class Entity<TId> : BaseEntity<TId>
{
    protected Entity()
    {
    }

    protected Entity(TId id) : base()
    {
        Id = id;
    }

    public TId Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is Entity<TId> entity)
        {
            return EqualityComparer<TId>.Default.Equals(Id, entity.Id);
        }
        return false;
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