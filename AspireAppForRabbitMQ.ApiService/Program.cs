using Aspire.RabbitMQ.Client;
using Microsoft.AspNetCore.OpenApi; // ensures OpenApi types/extensions are visible
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection; // <-- Add this using directive
using Microsoft.OpenApi.Models; // <-- Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Register RabbitMQ client (name must match AppHost resource: "rabbitmq")
builder.AddRabbitMQClient("rabbitmq");

// OpenAPI document for endpoints
builder.Services.AddEndpointsApiExplorer(); // <-- Add this line
builder.Services.AddSwaggerGen(c => // <-- Add this line
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspireAppWithRabbitMG API", Version = "v1" });
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Serve OpenAPI JSON and Scalar UI
app.UseSwagger(); // <-- Add this line
app.UseSwaggerUI(c => // <-- Add this line
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspireAppWithRabbitMG API v1");
    c.RoutePrefix = "scalar";
});

app.MapOpenApi(); // /openapi/v1.json
app.MapScalarApiReference(options =>
{
    options.Title = "AspireAppWithRabbitMG API";
});

// Redirect root to the UI
app.MapGet("/", () => Results.Redirect("/scalar"))
   .ExcludeFromDescription();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

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
})
.WithName("GetWeatherForecast")
.WithOpenApi(op =>
{
    op.Summary = "Returns a 5‑day weather forecast";
    op.Description = "Demonstrates minimal API + OpenAPI in .NET 8.";
    return op;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}