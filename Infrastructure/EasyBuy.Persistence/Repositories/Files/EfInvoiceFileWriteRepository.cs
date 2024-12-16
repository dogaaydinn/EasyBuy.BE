using EasyBuy.Application.Repositories.File;
using EasyBuy.Domain.Entities;
using EasyBuy.Persistence.Contexts;

namespace EasyBuy.Persistence.Repositories.Files;

public class EfInvoiceFileWriteRepository(EasyBuyDbContext dbContext)
    : EfWriteRepository<InvoiceFile>(dbContext), IInvoiceFileWriteRepository
{
}