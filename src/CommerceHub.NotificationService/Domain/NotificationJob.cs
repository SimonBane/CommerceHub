namespace CommerceHub.NotificationService.Domain;

/// <summary>
/// DDD aggregate root representing a notification to be sent.
/// State changes only through behavior methods. Stored in Marten for outbox pattern.
/// </summary>
public sealed class NotificationJob
{
    public Guid Id { get; private set; }
    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationKind Kind { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public NotificationStatus Status { get; private set; }

    public NotificationJob() { }

    public static NotificationJob Create(
        Guid recipientId,
        NotificationType type,
        NotificationKind kind,
        string subject,
        string body,
        string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));

        return new NotificationJob
        {
            Id = Guid.CreateVersion7(),
            RecipientId = recipientId,
            Type = type,
            Kind = kind,
            Subject = subject,
            Body = body,
            CorrelationId = correlationId,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Pending
        };
    }

    public void MarkAsSent()
    {
        if (!CanBeSent)
            throw new InvalidOperationException($"Cannot mark as sent. Current status: {Status}.");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        if (Status == NotificationStatus.Sent)
            throw new InvalidOperationException("Cannot mark as failed; notification was already sent.");

        Status = NotificationStatus.Failed;
    }

    public bool CanBeSent => Status == NotificationStatus.Pending;
}
