using FluentValidation;

namespace CommerceHub.OrderingService.Features.Orders.PayOrder;

public sealed class PayOrderValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(3);
    }
}
