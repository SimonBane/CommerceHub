using CommerceHub.PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.PaymentService.Features.Webhook;

/// <summary>
/// Webhook endpoint for external payment providers (e.g. Stripe). For MVP, accepts simulated webhooks.
/// Real implementation would verify signatures and use provider-specific payloads.
/// </summary>
public static class WebhookHandler
{
    [WolverinePost("/payment/webhooks/{provider}")]
    public static async Task<IResult> Handle(
        string provider,
        WebhookPayload payload,
        PaymentDbContext db,
        CancellationToken ct)
    {
        // For simulated gateway: if we have a transaction ID, try to find and update payment
        if (!string.IsNullOrEmpty(payload.TransactionId))
        {
            var payment = await db.Payments
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == payload.TransactionId, ct);

            if (payment != null)
            {
                if (payload.Status?.Equals("succeeded", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (payment.Status != Domain.PaymentStatus.Authorized)
                    {
                        payment.MarkAuthorized(payload.TransactionId);
                        await db.SaveChangesAsync(ct);
                    }
                }
                else if (payload.Status?.Equals("failed", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (payment.Status == Domain.PaymentStatus.Pending)
                    {
                        payment.MarkFailed(payload.Reason ?? "Webhook reported failure");
                        await db.SaveChangesAsync(ct);
                    }
                }
            }
        }

        // Always return 200 to acknowledge receipt (providers retry on non-2xx)
        return Results.Ok();
    }
}
