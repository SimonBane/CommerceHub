using FluentValidation;

namespace CommerceHub.OrderingService.Features.Orders.ShipOrder;

public sealed class ShipOrderValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderValidator()
    {
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(100);
    }
}
