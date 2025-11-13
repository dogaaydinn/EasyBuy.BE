# EasyBuy.BE - Unit Tests

This directory contains comprehensive unit tests for the EasyBuy.BE enterprise e-commerce platform.

## Test Projects

### EasyBuy.Application.Tests
Unit tests for the Application layer (CQRS handlers, validators, services).

**Test Coverage:**
- ✅ **Week 1:** Authentication & Authorization
  - RegisterCommandHandler (6 tests)
  - LoginCommandHandler (6 tests)
  - Token generation and validation

- ✅ **Week 2:** Order & Basket Management
  - CreateOrderCommandHandler (7 tests)
  - AddToBasketCommandHandler (5 tests)
  - Stock management
  - Payment processing
  - Coupon application

**Total Test Count:** 24+ unit tests

## Test Framework Stack

- **xUnit** 2.9.2 - Testing framework
- **Moq** 4.20.72 - Mocking framework
- **FluentAssertions** 6.12.0 - Assertion library
- **EntityFrameworkCore.InMemory** 9.0.0 - In-memory database for testing
- **.NET 9.0** - Target framework

## Running Tests

### Run All Tests
```bash
# From repository root
dotnet test

# From Tests directory
cd Tests
dotnet test

# From specific test project
cd Tests/EasyBuy.Application.Tests
dotnet test
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~RegisterCommandHandlerTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~RegisterCommandHandlerTests.Handle_ValidCommand_ShouldRegisterUser"
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity detailed
```

## Test Structure

```
Tests/
└── EasyBuy.Application.Tests/
    ├── Features/
    │   ├── Auth/
    │   │   └── Commands/
    │   │       ├── RegisterCommandHandlerTests.cs
    │   │       └── LoginCommandHandlerTests.cs
    │   ├── Orders/
    │   │   └── Commands/
    │   │       └── CreateOrderCommandHandlerTests.cs
    │   └── Baskets/
    │       └── Commands/
    │           └── AddToBasketCommandHandlerTests.cs
    ├── Helpers/
    │   └── TestDbContextFactory.cs
    └── GlobalUsings.cs
```

## Test Naming Convention

Tests follow the **Arrange-Act-Assert (AAA)** pattern and use descriptive naming:

```
MethodName_Scenario_ExpectedBehavior
```

**Examples:**
- `Handle_ValidCommand_ShouldRegisterUser`
- `Handle_DuplicateEmail_ShouldReturnFailure`
- `Handle_InsufficientStock_ShouldReturnFailure`

## Test Categories

### 1. Happy Path Tests
Tests that verify successful operations with valid data.

**Example:**
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldRegisterUser()
{
    // Arrange
    var command = new RegisterCommand { /* valid data */ };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Should().NotBeNull();
}
```

### 2. Validation Tests
Tests that verify input validation and business rules.

**Example:**
```csharp
[Fact]
public async Task Handle_DuplicateEmail_ShouldReturnFailure()
{
    // Verifies email uniqueness constraint
}
```

### 3. Error Handling Tests
Tests that verify proper error handling and error messages.

**Example:**
```csharp
[Fact]
public async Task Handle_InvalidCredentials_ShouldReturnFailure()
{
    // Verifies authentication failure handling
}
```

### 4. Business Logic Tests
Tests that verify complex business rules and calculations.

**Example:**
```csharp
[Fact]
public async Task Handle_ValidCouponCode_ShouldApplyDiscount()
{
    // Verifies discount calculation logic
}
```

## Test Helpers

### TestDbContextFactory
Provides methods for creating and managing in-memory database contexts for testing.

**Methods:**
- `Create()` - Creates a new in-memory EasyBuyDbContext
- `Destroy(context)` - Disposes and deletes the test database
- `GetMockUserManager()` - Creates a mocked UserManager
- `GetMockSignInManager()` - Creates a mocked SignInManager
- `GetMockRoleManager()` - Creates a mocked RoleManager

**Usage:**
```csharp
public class MyTests : IDisposable
{
    private readonly EasyBuyDbContext _context;

    public MyTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
```

## Mock Setup Patterns

### Mocking ICurrentUserService
```csharp
var mockCurrentUserService = new Mock<ICurrentUserService>();
mockCurrentUserService.Setup(x => x.UserId).Returns(testUserId);
mockCurrentUserService.Setup(x => x.IsInRole("Admin")).Returns(true);
```

### Mocking UserManager
```csharp
var mockUserManager = TestDbContextFactory.GetMockUserManager();
mockUserManager.Setup(x => x.FindByEmailAsync(email))
    .ReturnsAsync(user);
mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), password))
    .ReturnsAsync(IdentityResult.Success);
```

### Mocking IPaymentService
```csharp
var mockPaymentService = new Mock<IPaymentService>();
mockPaymentService.Setup(x => x.ProcessPaymentAsync(
    It.IsAny<Guid>(),
    It.IsAny<decimal>(),
    It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);
```

## Test Data Patterns

### Creating Test Products
```csharp
var product = new Product
{
    Id = Guid.NewGuid(),
    Name = "Test Product",
    Price = 100m,
    Stock = 10,
    IsActive = true
};
_context.Products.Add(product);
await _context.SaveChangesAsync();
```

### Creating Test Users
```csharp
var user = new AppUser
{
    Id = Guid.NewGuid(),
    Email = "test@example.com",
    UserName = "testuser",
    FirstName = "Test",
    LastName = "User"
};
```

### Creating Test Orders
```csharp
var order = new Order
{
    Id = Guid.NewGuid(),
    OrderNumber = "ORD-20250113-TEST",
    UserId = testUserId,
    Total = 100m,
    Status = OrderStatus.Pending
};
```

## Assertions with FluentAssertions

### Basic Assertions
```csharp
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
result.Error.Should().Contain("expected error message");
```

### Collection Assertions
```csharp
items.Should().HaveCount(5);
items.Should().Contain(x => x.Id == expectedId);
items.Should().BeInAscendingOrder(x => x.Name);
```

### Numeric Assertions
```csharp
total.Should().Be(100m);
total.Should().BeGreaterThan(0);
total.Should().BeInRange(90m, 110m);
```

### String Assertions
```csharp
message.Should().StartWith("ORD-");
message.Should().Contain("success");
message.Should().NotBeNullOrEmpty();
```

## Continuous Integration

Tests are automatically run on:
- Every commit to main branch
- Every pull request
- Nightly builds

**Required Test Pass Rate:** 100%

## Test Coverage Goals

| Layer | Target Coverage |
|-------|----------------|
| Application (Handlers) | 90%+ |
| Domain (Business Logic) | 85%+ |
| API (Controllers) | 80%+ |
| Overall | 85%+ |

## Future Test Additions

### Planned for Week 3:
- [ ] Product search query tests
- [ ] Review command tests
- [ ] Wishlist command tests
- [ ] Notification service tests

### Planned for Week 4:
- [ ] Integration tests
- [ ] Performance tests
- [ ] Load tests
- [ ] End-to-end API tests

## Troubleshooting

### Tests Fail with "Database Already Exists"
Each test creates a unique in-memory database using `Guid.NewGuid().ToString()`.
If tests fail, ensure you're disposing the context properly.

### Mock Setup Not Working
Verify you're using the correct setup method:
- For properties: `.Setup(x => x.Property).Returns(value)`
- For methods: `.Setup(x => x.Method(...)).ReturnsAsync(value)`

### InMemory Database Limitations
InMemory database doesn't support:
- SQL Server specific features
- True relational constraints
- Some complex queries

For these scenarios, use a real test database or Testcontainers.

## Contributing

When adding new tests:
1. Follow the AAA pattern
2. Use descriptive test names
3. Test both success and failure cases
4. Add XML comments for complex tests
5. Ensure proper cleanup in Dispose()
6. Update this README if adding new test categories

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [EF Core InMemory Provider](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/)
