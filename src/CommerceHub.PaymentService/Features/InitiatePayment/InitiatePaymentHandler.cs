using CommerceHub.Contracts.Payment;
using CommerceHub.PaymentService.Domain;
using CommerceHub.PaymentService.Infrastructure.PaymentGateway;
using CommerceHub.PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace CommerceHub.PaymentService.Features.InitiatePayment;

/// <summary>
/// Handles InitiatePaymentCommand from CheckoutOrchestrator. Calls payment gateway and publishes result event.
/// </summary>
public static class InitiatePaymentHandler
{
    public static async Task Handle(
        InitiatePaymentCommand command,
        IDbContextOutbox<PaymentDbContext> outbox,
        IPaymentGateway gateway,
        CancellationToken ct)
    {
        var db = outbox.DbContext;

        // Idempotency: if we already have a payment for this order, return the existing result
        var existingPayment = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == command.OrderId, ct);
        if (existingPayment != null)
        {
            if (existingPayment.Status == PaymentStatus.Authorized)
            {
                await outbox.PublishAsync(new PaymentAuthorizedV1(
                    existingPayment.Id,
                    command.OrderId,
                    command.CustomerId,
                    existingPayment.Amount,
                    existingPayment.CurrencyCode,
                    existingPayment.ExternalTransactionId,
                    existingPayment.AuthorizedAt!.Value));
            }
            else if (existingPayment.Status == PaymentStatus.Failed)
            {
                await outbox.PublishAsync(new PaymentFailedV1(
                    command.OrderId,
                    existingPayment.FailureReason ?? "Payment failed",
                    DateTime.UtcNow));
            }
            await outbox.SaveChangesAndFlushMessagesAsync(ct);
            return;
        }

        var payment = new Payment(command.OrderId, command.CustomerId, command.Amount, command.CurrencyCode);
        db.Payments.Add(payment);

        var gatewayRequest = new PaymentGatewayRequest(
            command.OrderId,
            command.CustomerId,
            command.Amount,
            command.CurrencyCode);

        var result = await gateway.AuthorizeAsync(gatewayRequest, ct);

        if (result.Success)
        {
            payment.MarkAuthorized(result.TransactionId);
            await outbox.PublishAsync(new PaymentAuthorizedV1(
                payment.Id,
                command.OrderId,
                command.CustomerId,
                command.Amount,
                command.CurrencyCode,
                result.TransactionId,
                DateTime.UtcNow));
        }
        else
        {
            payment.MarkFailed(result.FailureReason ?? "Unknown error");
            await outbox.PublishAsync(new PaymentFailedV1(
                command.OrderId,
                result.FailureReason ?? "Payment authorization failed",
                DateTime.UtcNow));
        }

        await outbox.SaveChangesAndFlushMessagesAsync(ct);
    }
}
