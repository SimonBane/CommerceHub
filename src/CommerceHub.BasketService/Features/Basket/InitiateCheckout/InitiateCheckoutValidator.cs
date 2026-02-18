using FluentValidation;

namespace CommerceHub.BasketService.Features.Basket.InitiateCheckout;

public sealed class InitiateCheckoutValidator : AbstractValidator<InitiateCheckoutCommand>
{
    public InitiateCheckoutValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
