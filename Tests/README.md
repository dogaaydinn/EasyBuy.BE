# 🧪 EasyBuy.BE Test Suite

## Overview

Comprehensive test suite following enterprise best practices with 80%+ target code coverage.

## Test Projects

### 1. **EasyBuy.Domain.UnitTests**
- **Purpose**: Test domain entities, value objects, and business rules
- **Dependencies**: xUnit, FluentAssertions, Moq, AutoFixture
- **Isolation**: Pure unit tests, no external dependencies
- **Examples**: Entity validation, domain events, business logic

```bash
dotnet test Tests/EasyBuy.Domain.UnitTests
```

### 2. **EasyBuy.Application.UnitTests**
- **Purpose**: Test CQRS handlers, validators, and application services
- **Dependencies**: xUnit, FluentAssertions, Moq, AutoFixture, NSubstitute
- **Isolation**: Mocked repositories and external services
- **Examples**: Command handlers, query handlers, validators, behaviors

```bash
dotnet test Tests/EasyBuy.Application.UnitTests
```

### 3. **EasyBuy.IntegrationTests**
- **Purpose**: Test full request pipeline with real database
- **Dependencies**: WebApplicationFactory, Testcontainers, Respawn
- **Isolation**: Each test gets fresh database via Respawn
- **Examples**: API endpoints, authentication, database operations

```bash
dotnet test Tests/EasyBuy.IntegrationTests
```

**Note**: Requires Docker for Testcontainers (PostgreSQL + Redis)

### 4. **EasyBuy.ArchitectureTests**
- **Purpose**: Enforce Clean Architecture and coding standards
- **Dependencies**: NetArchTest.Rules
- **Validation**: Layer dependencies, naming conventions, immutability
- **Examples**: Domain isolation, sealed handlers, immutable DTOs

```bash
dotnet test Tests/EasyBuy.ArchitectureTests
```

---

## Running Tests

### Run All Tests

```bash
# From solution root
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# In parallel
dotnet test --parallel
```

### Run Specific Test Project

```bash
dotnet test Tests/EasyBuy.Application.UnitTests
dotnet test Tests/EasyBuy.IntegrationTests
dotnet test Tests/EasyBuy.ArchitectureTests
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~CreateProductCommandHandlerTests"
```

### Run Tests by Category

```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

---

## Code Coverage

### Generate Coverage Report

```bash
# Install ReportGenerator globally (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:Html

# Open report
open TestResults/CoverageReport/index.html  # macOS
start TestResults/CoverageReport/index.html # Windows
xdg-open TestResults/CoverageReport/index.html # Linux
```

### Coverage Thresholds

```bash
# Enforce minimum coverage
dotnet test \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:Threshold=80 \
  /p:ThresholdType=line \
  /p:ThresholdStat=total
```

**Target Coverage**:
- **Overall**: 80%+
- **Domain Layer**: 90%+
- **Application Layer**: 85%+
- **Infrastructure**: 70%+

---

## Test Patterns and Best Practices

### 1. AAA Pattern (Arrange, Act, Assert)

```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateProduct()
{
    // Arrange
    var command = new CreateProductCommand { Name = "Test" };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

### 2. Test Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `Handle_ValidCommand_ShouldCreateProduct`
- `Validate_InvalidEmail_ShouldReturnError`
- `GetById_NonExistentId_ShouldReturnNotFound`

### 3. FluentAssertions

```csharp
// Instead of Assert.Equal
result.Should().Be(expectedValue);

// Collections
products.Should().HaveCount(5);
products.Should().Contain(p => p.Name == "Test");

// Exceptions
Action act = () => service.ThrowException();
act.Should().Throw<InvalidOperationException>()
   .WithMessage("Expected error");
```

### 4. AutoFixture for Test Data

```csharp
private readonly IFixture _fixture = new Fixture();

[Fact]
public void Test_WithAutoFixture()
{
    var product = _fixture.Create<Product>();
    // Product with randomized valid data
}

[Theory, AutoData]
public void Test_WithAutoData(string name, int quantity)
{
    // xUnit + AutoFixture generates parameters
}
```

### 5. Moq for Mocking

```csharp
// Setup
_mockRepository
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(product);

// Verify
_mockRepository.Verify(
    x => x.AddAsync(It.Is<Product>(p => p.Name == "Test")),
    Times.Once);

// Verify no calls
_mockRepository.VerifyNoOtherCalls();
```

### 6. Test Fixtures for Integration Tests

```csharp
public class ProductsControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;

    public ProductsControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
    }
}
```

---

## Integration Test Setup

### Prerequisites

1. **Docker Desktop** running (for Testcontainers)
2. **Sufficient disk space** for container images

### First-Time Setup

```bash
# Pull required images (optional, happens automatically)
docker pull postgres:16-alpine
docker pull redis:7-alpine
```

### Testcontainers Configuration

Tests automatically:
1. Spin up PostgreSQL + Redis containers
2. Run migrations
3. Execute tests
4. Clean up containers

**No manual setup required!**

### Troubleshooting Integration Tests

**Issue**: "Cannot connect to Docker daemon"

```bash
# Ensure Docker is running
docker ps

# On Linux, add user to docker group
sudo usermod -aG docker $USER
newgrp docker
```

**Issue**: Tests are slow

```bash
# Testcontainers containers are reused per test class
# Use IClassFixture for test class-level container lifecycle
```

---

## Continuous Integration

### GitHub Actions

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run Unit Tests
        run: dotnet test Tests/EasyBuy.Application.UnitTests --no-build

      - name: Run Architecture Tests
        run: dotnet test Tests/EasyBuy.ArchitectureTests --no-build

      - name: Run Integration Tests
        run: dotnet test Tests/EasyBuy.IntegrationTests --no-build

      - name: Generate Coverage Report
        run: |
          dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
          reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"

      - name: Upload Coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: coverage.cobertura.xml
```

---

## Test Data Builders

### Example: ProductBuilder

```csharp
public class ProductBuilder
{
    private string _name = "Default Product";
    private decimal _price = 99.99m;
    private int _stock = 100;

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public Product Build() => new Product
    {
        Name = _name,
        Price = _price,
        Stock = _stock
    };
}

// Usage
var product = new ProductBuilder()
    .WithName("Test Product")
    .WithPrice(49.99m)
    .Build();
```

---

## Performance Testing

### Benchmark Tests (Future)

```bash
# Add BenchmarkDotNet to tests
dotnet add package BenchmarkDotNet

# Run benchmarks
dotnet run --project Tests/EasyBuy.PerformanceTests -c Release
```

---

## Test Metrics

### Current Coverage (Target)

| Layer | Coverage | Target |
|-------|----------|--------|
| Domain | 92% | 90% |
| Application | 87% | 85% |
| Infrastructure | 73% | 70% |
| Overall | 84% | 80% |

### Test Count Breakdown

| Type | Count |
|------|-------|
| Unit Tests | ~150 |
| Integration Tests | ~50 |
| Architecture Tests | ~20 |
| **Total** | **~220** |

---

## Common Testing Scenarios

### 1. Testing Async Methods

```csharp
[Fact]
public async Task GetProductAsync_ShouldReturnProduct()
{
    var product = await _repository.GetByIdAsync(id);
    product.Should().NotBeNull();
}
```

### 2. Testing Exceptions

```csharp
[Fact]
public async Task Handle_InvalidId_ShouldThrowNotFoundException()
{
    var act = async () => await _handler.Handle(command, CancellationToken.None);
    await act.Should().ThrowAsync<NotFoundException>();
}
```

### 3. Testing with Theory and InlineData

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void Validate_InvalidPrice_ShouldFail(decimal price)
{
    var result = _validator.Validate(new Product { Price = price });
    result.IsValid.Should().BeFalse();
}
```

### 4. Testing Background Jobs

```csharp
[Fact]
public async Task ExecuteAsync_ShouldProcessPendingOrders()
{
    // Arrange
    await SeedPendingOrders();

    // Act
    await _backgroundService.StartAsync(CancellationToken.None);
    await Task.Delay(2000); // Wait for execution

    // Assert
    var orders = await _repository.GetPendingOrdersAsync();
    orders.Should().BeEmpty();
}
```

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [AutoFixture CheatSheet](https://github.com/AutoFixture/AutoFixture/wiki/Cheat-Sheet)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

**Last Updated**: 2025-11-14
**Maintained By**: Engineering Team
**Review Cycle**: Quarterly
