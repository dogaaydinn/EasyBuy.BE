using EasyBuy.Application.Repositories.File;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.File;

public class EfFileReadRepository(EasyBuyDbContext dbContext) : IFileReadRepository<Domain.Entities.File>(dbContext), IFileReadRepository
{
}
