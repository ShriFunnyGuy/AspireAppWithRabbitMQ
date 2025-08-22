# .NET Aspire Blazor + RabbitMQ Sample

A distributed .NET 8 Aspire app demonstrating:
- Blazor Server (webfrontend)
- Minimal API (apiservice) that publishes messages to RabbitMQ
- Background worker (consumer) that consumes messages from RabbitMQ
- Shared service defaults (health, telemetry) and common model project

RabbitMQ is provisioned and wired via Aspire AppHost.

## Projects

- AspireAppForRabbitMG.AppHost
  - Orchestrates the solution and RabbitMQ resource (with management UI).
- AspireAppForRabbitMG.Web
  - Blazor Server app. Calls the API for weather data and uses Redis output cache.
- AspireAppWithRabbitMG.ApiService
  - Minimal API. Exposes `/weatherforecast` and `/send-message` (publishes `OrdersModel` to RabbitMQ).
- AspireRabbitMQConsumer
  - BackgroundService that subscribes to the queue and logs received messages.
- AspireAppForRabbitMG.ServiceDefaults
  - Common observability, health, and resilience configuration.
- AspireAppWithRabbitMG.Common
  - Shared types (e.g., `OrdersModel`).

## Prerequisites

- .NET 8 SDK
- Docker Desktop (required by Aspire to run RabbitMQ and dependencies)
- Visual Studio 2022 17.9+ (recommended) or `dotnet` CLI

## Quick start

1) Restore and build

2) Run the distributed app (this starts RabbitMQ, the API, the consumer, and the Blazor app)

3) Open the Blazor UI  
The AppHost output (or Visual Studio Aspire dashboard) shows the public URL for `webfrontend`.

4) Send a test message  
The API exposes `GET /send-message` which publishes an `OrdersModel` JSON to RabbitMQ. If the API is not externally exposed, see the “Expose the API externally” section below.

5) Observe the consumer logs  
The consumer logs each message it receives.

## RabbitMQ

- Queue name is configured via `RabbitMQ:QueueName`. The API defaults to `messages`. Ensure the consumer uses the same queue name.
- RabbitMQ Management UI is enabled in AppHost (`WithManagementPlugin()`). Open it from the Aspire dashboard.

Example appsettings.json (ensure both publisher and consumer match):

## API endpoints

- GET /weatherforecast
- GET /send-message
  - Publishes a JSON payload like:
  ```
  {
    "id": 42,
    "item": "Widget",
    "quantity": 10
  }
  ```

## Expose the API externally (optional)

By default, the API is internal to the Aspire network. To call `/send-message` from a browser/curl, make it external:

In `AspireAppForRabbitMG.AppHost/AppHost.cs` add `.WithExternalHttpEndpoints()` to the API project:

Run the AppHost again and use the external URL for `GET /send-message`.

## Gotchas

- Queue name mismatch
  - API defaults to `messages`; consumer originally defaulted to `orders`. Set `RabbitMQ:QueueName=messages` in both to avoid silence.
- Durability
  - Demo uses non-durable queues and auto-ack in the consumer (simpler, not production-safe). For reliability, switch to durable queues/messages and manual acks.
- Docker must be running
  - Aspire will spin up RabbitMQ; ensure Docker Desktop is started.

## Development

- Visual Studio: Set AppHost as startup project and press F5. Use the Aspire dashboard to discover service endpoints.
- CLI: `dotnet run --project AspireAppForRabbitMG.AppHost`.

## License

MIT (update as appropriate).