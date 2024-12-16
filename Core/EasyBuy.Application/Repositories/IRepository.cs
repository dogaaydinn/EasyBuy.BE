using EasyBuy.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    DbSet<T> DbSet { get; set; }
}