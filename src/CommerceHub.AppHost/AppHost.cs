using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitMq = builder.AddRabbitMQ("rabbitMq")
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongoDb = builder.AddMongoDB("mongoDb", 27017)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogDb");

var catalogService = builder.AddProject<Projects.CommerceHub_CatalogService>("commercehub-catalogService")
    .WithReference(catalogDb)
    .WithReference(rabbitMq)
    .WithReference(keycloak)
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq)
    .WaitFor(keycloak);

var searchService = builder.AddProject<Projects.CommerceHub_SearchService>("commercehub-searchService")
    .WithReference(mongoDb)
    .WithReference(rabbitMq)
    .WithReference(keycloak)
    .WaitFor(mongoDb)
    .WaitFor(rabbitMq)
    .WaitFor(keycloak);

var basketService = builder.AddProject<Projects.CommerceHub_BasketService>("commercehub-basketService")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(redis)
    .WaitFor(rabbitMq);

// var orderingDb = postgres.AddDatabase("orderingDb");
//
// var inventoryDb = postgres.AddDatabase("inventoryDb");
//
// var paymentDb = postgres.AddDatabase("paymentDb");
//
// var checkoutOrchestratorDb = postgres.AddDatabase("checkoutOrchestratorDb");
//
// var checkoutOrchestrator = builder.AddProject<Projects.CommerceHub_CheckoutOrchestrator>("commercehub-checkoutorchestrator")
//     .WithReference(checkoutOrchestratorDb)
//     .WithReference(rabbitMq)
//     .WaitFor(checkoutOrchestratorDb)
//     .WaitFor(rabbitMq);
//
// var ordering = builder.AddProject<Projects.CommerceHub_OrderingService>("commercehub-orderingservice")
//     .WithReference(orderingDb)
//     .WithReference(rabbitMq)
//     .WaitFor(orderingDb)
//     .WaitFor(rabbitMq);
//
// var inventory = builder.AddProject<Projects.CommerceHub_InventoryService>("commercehub-inventoryservice")
//     .WithReference(inventoryDb)
//     .WithReference(rabbitMq)
//     .WaitFor(inventoryDb)
//     .WaitFor(rabbitMq);
//
// var payment = builder.AddProject<Projects.CommerceHub_PaymentService>("commercehub-paymentservice")
//     .WithReference(paymentDb)
//     .WithReference(rabbitMq)
//     .WaitFor(paymentDb)
//     .WaitFor(rabbitMq);
//
// var notificationDb = postgres.AddDatabase("notificationDb");
//
// var notification = builder.AddProject<Projects.CommerceHub_NotificationService>("commercehub-notificationservice")
//     .WithReference(notificationDb)
//     .WithReference(rabbitMq)
//     .WaitFor(notificationDb)
//     .WaitFor(rabbitMq);
//
// var backoffice = builder.AddProject<Projects.CommerceHub_BackofficeApi>("commercehub-backofficeapi")
//     .WithReference(mongoDb)
//     .WithReference(catalog)
//     .WithReference(ordering)
//     .WaitFor(mongoDb)
//     .WaitFor(catalog)
//     .WaitFor(ordering);

// builder.AddProject<Projects.CommerceHub_GatewayApi>("commercehub-gateway")
//     .WithReference(catalog)
//     .WithReference(search)
//     .WithReference(basket)
//     // .WithReference(checkoutOrchestrator)
//     // .WithReference(notification)
//     // .WithReference(backoffice)
//     // .WithReference(ordering)
//     // .WithReference(inventory)
//     // .WithReference(payment)
//     .WaitFor(catalog)
//     .WaitFor(search)
//     .WaitFor(basket);
// // .WaitFor(checkoutOrchestrator)
// // .WaitFor(notification)
// // .WaitFor(backoffice)
// // .WaitFor(ordering)
// // .WaitFor(inventory)
// // .WaitFor(payment);

builder.AddYarp("gateway")
    .WithHttpsEndpoint(5001, 5001, "https")
    .WithConfiguration(config =>
    {
        config.AddRoute("/api/catalog/{**catch-all}", catalogService)
            .WithTransformPathRemovePrefix("/api");
        
        config.AddRoute("/api/search/{**catch-all}", searchService)
            .WithTransformPathRemovePrefix("/api");
        
        config.AddRoute("/api/basket/{**catch-all}", basketService)
            .WithTransformPathRemovePrefix("/api");
    });

builder.Build().Run();