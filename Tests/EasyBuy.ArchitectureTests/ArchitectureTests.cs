using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace EasyBuy.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "EasyBuy.Domain";
    private const string ApplicationNamespace = "EasyBuy.Application";
    private const string InfrastructureNamespace = "EasyBuy.Infrastructure";
    private const string PersistenceNamespace = "EasyBuy.Persistence";
    private const string WebApiNamespace = "EasyBuy.WebAPI";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Domain.Primitives.BaseEntity).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                PersistenceNamespace,
                WebApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on other layers");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrPresentation()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                PersistenceNamespace,
                WebApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure or Presentation layers");
    }

    [Fact]
    public void Handlers_ShouldHaveDependencyOnDomain()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("EasyBuy.Application.Features")
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Handlers should depend on Domain layer");
    }

    [Fact]
    public void Controllers_ShouldHaveDependencyOnMediatR()
    {
        // Arrange
        var assembly = typeof(EasyBuy.WebAPI.Controllers.ProductsController).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .HaveDependencyOn("MediatR")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Controllers should use MediatR");
    }

    [Fact]
    public void Validators_ShouldHaveCorrectNamingConvention()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Validators should end with 'Validator'");
    }

    [Fact]
    public void CommandHandlers_ShouldHaveCorrectNamingConvention()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining(".Commands.")
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Command handlers should end with 'CommandHandler'");
    }

    [Fact]
    public void QueryHandlers_ShouldHaveCorrectNamingConvention()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining(".Queries.")
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Query handlers should end with 'QueryHandler'");
    }

    [Fact]
    public void Entities_ShouldResideInDomainLayer()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Domain.Primitives.BaseEntity).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(EasyBuy.Domain.Primitives.BaseEntity))
            .Should()
            .ResideInNamespace(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All entities should reside in Domain layer");
    }

    [Fact]
    public void DomainEvents_ShouldBeSealed()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Domain.Primitives.BaseEntity).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("EasyBuy.Domain.Events")
            .And()
            .HaveNameEndingWith("Event")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain events should be sealed");
    }

    [Fact]
    public void Commands_ShouldBeClasses()
    {
        // Arrange
        var assembly = typeof(EasyBuy.Application.ServiceRegistration).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining(".Commands.")
            .And()
            .HaveNameEndingWith("Command")
            .Should()
            .BeClasses()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Commands should be classes");
    }
}
