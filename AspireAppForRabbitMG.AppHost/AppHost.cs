using Aspire.Hosting.RabbitMQ;

var builder = DistributedApplication.CreateBuilder(args);

// Redis used by the Blazor app
var cache = builder.AddRedis("cache");

// Host RabbitMQ as a managed resource
var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var apiService = builder.AddProject<Projects.AspireAppWithRabbitMQ_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rabbitMQ)   // inject RabbitMQ binding into the API
    .WaitFor(rabbitMQ);

builder.AddProject<Projects.AspireAppWithRabbitMQ_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.AspireRabbitMQConsumer>("consumer")
.WithReference(rabbitMQ)
.WaitFor(rabbitMQ);

builder.Build().Run();
