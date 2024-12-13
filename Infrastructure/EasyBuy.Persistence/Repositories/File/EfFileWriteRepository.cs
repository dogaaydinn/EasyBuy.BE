using EasyBuy.Application.Repositories.File;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.File;

public class EfFileWriteRepository(EasyBuyDbContext dbContext) : IFileWriteRepository<Domain.Entities.File>(dbContext), IFileWriteRepository
{
}