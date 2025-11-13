using EasyBuy.Domain.Entities.Identity;
using EasyBuy.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyBuy.Application.Tests.Helpers;

public static class TestDbContextFactory
{
    public static EasyBuyDbContext Create()
    {
        var options = new DbContextOptionsBuilder<EasyBuyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new EasyBuyDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    public static void Destroy(EasyBuyDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    public static Mock<UserManager<AppUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var mockUserManager = new Mock<UserManager<AppUser>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        return mockUserManager;
    }

    public static Mock<SignInManager<AppUser>> GetMockSignInManager(Mock<UserManager<AppUser>> userManager)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();

        var mockSignInManager = new Mock<SignInManager<AppUser>>(
            userManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null,
            null,
            null,
            null);

        return mockSignInManager;
    }

    public static Mock<RoleManager<AppRole>> GetMockRoleManager()
    {
        var store = new Mock<IRoleStore<AppRole>>();
        var mockRoleManager = new Mock<RoleManager<AppRole>>(
            store.Object,
            null,
            null,
            null,
            null);

        return mockRoleManager;
    }
}
