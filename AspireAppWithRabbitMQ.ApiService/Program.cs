using AspireAppWithRabbitMQ.Common;
using RabbitMQ.Client;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // adds health, telemetry, resilience
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRabbitMQClient("rabbitmq");

var app = builder.Build();

app.MapDefaultEndpoints(); // exposes /health (and friends)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapGet("/send-message", async (IConnection messageConnection,IConfiguration configuration) =>
{
    const string configKeyName = "RabbitMQ:QueueName";
    string queueName = configuration[configKeyName] ?? "messages";
    var channel = messageConnection.CreateModel();
    channel.QueueDeclare(queue: queueName,
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
    var message = "Hello, RabbitMQ!";
    var body = System.Text.Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchange: "",
                         routingKey: queueName,
                         basicProperties: null,
                         //body: body);
                         body:JsonSerializer.SerializeToUtf8Bytes(
                             new OrdersModel {
                                 OrderId=$"Message from API:{Guid.NewGuid()}",
                                 ProductName="Book",
                                 Quantity=3
                             }
                             ));

    return Results.Ok($"Message sent: {message}");
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}