namespace CommerceHub.Contracts.Messaging;

/// <summary>
/// RabbitMQ queue names used for inter-service messaging.
/// </summary>
public static class QueueNames
{
    public const string BasketEvents = "basket-events";
    public const string CatalogEvents = "catalog-events";
    public const string InventoryCommands = "inventory-commands";
    public const string InventoryEvents = "inventory-events";
    public const string OrderCommands = "order-commands";
    public const string OrderEvents = "order-events";
    public const string PaymentCommands = "payment-commands";
    public const string PaymentEvents = "payment-events";
}
