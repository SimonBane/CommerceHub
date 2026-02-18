namespace CommerceHub.NotificationService.Constants;

/// <summary>
/// Notification template subject and body patterns.
/// </summary>
public static class NotificationTemplates
{
    public static class OrderPlaced
    {
        public const string Subject = "Order Confirmation - #{0}";
        public const string Body = "Thank you for your order! Your order #{0} has been placed successfully. Total: {1} {2}.";
    }

    public static class OrderPaid
    {
        public const string Subject = "Payment Received - Order #{0}";
        public const string Body = "We have received your payment for order #{0}. Amount: {1} {2}.";
    }

    public static class OrderShipped
    {
        public const string Subject = "Your Order Has Shipped - #{0}";
        public const string Body = "Great news! Your order #{0} has shipped. Tracking number: {1}";
    }

    public static class OrderCancelled
    {
        public const string Subject = "Order Cancelled - #{0}";
        public const string Body = "Your order #{0} has been cancelled. Reason: {1}";
    }
}
