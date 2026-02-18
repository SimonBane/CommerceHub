using CommerceHub.PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace CommerceHub.PaymentService.Features.GetPayment;

public static class GetPaymentHandler
{
    [WolverineGet("/payment/order/{orderId:guid}")]
    public static async Task<IResult> HandleByOrderId(Guid orderId, PaymentDbContext db, CancellationToken ct)
    {
        var payment = await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

        if (payment == null)
            return Results.NotFound();

        return Results.Ok(new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount,
            payment.CurrencyCode,
            payment.Status.ToString(),
            payment.ExternalTransactionId,
            payment.CreatedAt,
            payment.AuthorizedAt));
    }

    [WolverineGet("/payment/{paymentId:guid}")]
    public static async Task<IResult> HandleById(Guid paymentId, PaymentDbContext db, CancellationToken ct)
    {
        var payment = await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment == null)
            return Results.NotFound();

        return Results.Ok(new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount,
            payment.CurrencyCode,
            payment.Status.ToString(),
            payment.ExternalTransactionId,
            payment.CreatedAt,
            payment.AuthorizedAt));
    }
}

public sealed record PaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    string Status,
    string? ExternalTransactionId,
    DateTime CreatedAt,
    DateTime? AuthorizedAt);
