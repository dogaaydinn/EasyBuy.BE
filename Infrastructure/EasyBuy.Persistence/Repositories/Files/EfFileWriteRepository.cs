using EasyBuy.Application.Repositories.File;
using EasyBuy.Persistence.Contexts;
using File = EasyBuy.Domain.Entities.File;

namespace EasyBuy.Persistence.Repositories.Files;

public class EfFileWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<File>(dbContext), IFileWriteRepository
{
}