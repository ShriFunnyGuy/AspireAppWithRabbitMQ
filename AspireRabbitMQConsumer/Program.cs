using AspireRabbitMQConsumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddHostedService<ProcessRabbitMQMessage>();
var host = builder.Build();
host.Run();