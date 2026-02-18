using FluentValidation;

namespace CommerceHub.BasketService.Features.Basket.UpdateItem;

public sealed class UpdateItemValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}
