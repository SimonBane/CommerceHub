using FluentValidation;

namespace CommerceHub.InventoryService.Features.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Delta must not be zero.");
    }
}
