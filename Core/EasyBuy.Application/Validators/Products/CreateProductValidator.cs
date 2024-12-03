using EasyBuy.Application.ViewModels.Products;
using FluentValidation;

namespace EasyBuy.Application.Validators.Products;

public abstract class CreateProductValidator : AbstractValidator<VmCreateProduct>
{
    protected CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .MinimumLength(5)
            .WithMessage("Name must be between 5 and 100 characters");

        RuleFor(x => x.Price)
            .NotEmpty()
            .NotNull()
            .WithMessage("Price is required")
            .Must(p => p >= 0)
            .WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.Stock)
            .NotEmpty()
            .NotNull()
            .WithMessage("Stock is required")
            .Must(s => s >= 0)
            .WithMessage("Stock must be greater than or equal to 0");
        
    }
}