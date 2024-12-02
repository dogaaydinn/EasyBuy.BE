using EasyBuy.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application;

public interface IRepository<T> where T : BaseEntity
{
    DbSet<T> DbSet { get; set; }
    
}