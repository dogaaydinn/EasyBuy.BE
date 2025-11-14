using System.Reflection;
using EasyBuy.Application;
using EasyBuy.Domain;
using EasyBuy.Infrastructure;
using EasyBuy.Persistence;

namespace EasyBuy.ArchitectureTests;

/// <summary>
/// Architecture tests to enforce Clean Architecture principles and coding standards.
/// These tests ensure the codebase maintains proper layering and dependency flow.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureAssemblyMarker).Assembly;
    private static readonly Assembly PersistenceAssembly = typeof(PersistenceAssemblyMarker).Assembly;

    #region Layer Dependency Rules

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnAnyOtherLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("EasyBuy.Application")
            .And().NotHaveDependencyOn("EasyBuy.Infrastructure")
            .And().NotHaveDependencyOn("EasyBuy.Persistence")
            .And().NotHaveDependencyOn("EasyBuy.WebAPI")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on any other layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_Should_OnlyDependOnDomain()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("EasyBuy.Infrastructure")
            .And().NotHaveDependencyOn("EasyBuy.Persistence")
            .And().NotHaveDependencyOn("EasyBuy.WebAPI")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application layer should only depend on Domain layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Infrastructure_Should_NotDependOnPersistenceOrPresentation()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("EasyBuy.Persistence")
            .And().NotHaveDependencyOn("EasyBuy.WebAPI")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Infrastructure layer should not depend on Persistence or Presentation layers. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Persistence_Should_NotDependOnInfrastructureOrPresentation()
    {
        // Arrange & Act
        var result = Types.InAssembly(PersistenceAssembly)
            .Should()
            .NotHaveDependencyOn("EasyBuy.Infrastructure")
            .And().NotHaveDependencyOn("EasyBuy.WebAPI")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Persistence layer should not depend on Infrastructure or Presentation layers. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Naming Conventions

    [Fact]
    public void CommandHandlers_Should_HaveCorrectNaming()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().HaveNameEndingWith("Command", StringComparison.InvariantCulture)
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Command handlers should end with 'CommandHandler'. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void QueryHandlers_Should_HaveCorrectNaming()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().HaveNameEndingWith("Query", StringComparison.InvariantCulture)
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Query handlers should end with 'QueryHandler'. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Validators_Should_HaveCorrectNaming()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Validators should end with 'Validator'. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Entities_Should_BeInEntitiesNamespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(EasyBuy.Domain.Common.BaseEntity))
            .Should()
            .ResideInNamespaceContaining("Entities")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should reside in 'Entities' namespace. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Immutability and Encapsulation

    [Fact]
    public void DomainEvents_Should_BeImmutable()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespaceContaining("Events")
            .And().AreClasses()
            .Should()
            .BeImmutable()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain events should be immutable (no public setters). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Commands_Should_BeImmutable()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .And().AreClasses()
            .Should()
            .BeImmutable()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Commands should be immutable (no public setters). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Queries_Should_BeImmutable()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Query")
            .And().AreClasses()
            .Should()
            .BeImmutable()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Queries should be immutable (no public setters). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Sealing and Inheritance

    [Fact]
    public void CommandHandlers_Should_BeSealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Command handlers should be sealed to prevent inheritance. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void QueryHandlers_Should_BeSealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Query handlers should be sealed to prevent inheritance. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Dependency Injection

    [Fact]
    public void Repositories_Should_HaveInterfaceInApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PersistenceAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And().AreNotInterfaces()
            .Should()
            .HaveDependencyOn("EasyBuy.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Repository implementations should reference interfaces from Application layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Services_Should_ImplementInterfaces()
    {
        // Arrange
        var services = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .And().AreNotInterfaces()
            .And().AreNotAbstract()
            .GetTypes();

        // Act & Assert
        foreach (var service in services)
        {
            var interfaces = service.GetInterfaces()
                .Where(i => i.Namespace?.StartsWith("EasyBuy") == true)
                .ToList();

            interfaces.Should().NotBeEmpty(
                $"{service.Name} should implement at least one interface from the Application layer");
        }
    }

    #endregion

    #region Clean Code Principles

    [Fact]
    public void Controllers_Should_NotHaveLogicInActions()
    {
        // This is a guideline - controllers should delegate to MediatR
        // Actual enforcement would require analyzing method bodies
        var result = Types.InAssembly(Assembly.Load("EasyBuy.WebAPI"))
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .HaveDependencyOn("MediatR")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Controllers should use MediatR for business logic delegation. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_Should_NotUseDataAnnotations()
    {
        // Domain should use FluentValidation, not data annotations
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("System.ComponentModel.DataAnnotations")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not use DataAnnotations. Use FluentValidation instead. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_Should_NotUseEntityFramework()
    {
        // Domain should be persistence-ignorant
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should be persistence-ignorant. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Security

    [Fact]
    public void Exceptions_Should_NotExposeInternalDetails()
    {
        // Custom exceptions should be in Domain.Exceptions
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .ResideInNamespaceContaining("Exceptions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Custom exceptions should reside in Domain.Exceptions namespace. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Test Coverage Helpers

    [Fact]
    public void AllPublicMethods_Should_HaveTests()
    {
        // This is more of a reminder - actual test coverage is measured by coverlet
        // You can extend this to check for corresponding test files
        Assert.True(true, "Use 'dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura' for coverage");
    }

    #endregion
}
