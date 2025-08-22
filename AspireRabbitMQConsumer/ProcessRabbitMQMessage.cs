using AspireAppWithRabbitMQ.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace AspireRabbitMQConsumer
{
    public class ProcessRabbitMQMessage : BackgroundService
    {
        private readonly ILogger<ProcessRabbitMQMessage> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public ProcessRabbitMQMessage(ILogger<ProcessRabbitMQMessage> logger,IConfiguration configuration,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string configKeyName = "RabbitMQ:QueueName";
            string queueName = _configuration[configKeyName] ?? "message";
            _logger.LogInformation("Starting RabbitMQ message processing for queue: {QueueName}", queueName);
            using (var scope = _serviceProvider.CreateScope())
            {
                var connectionFactory = scope.ServiceProvider.GetRequiredService<IConnectionFactory>();
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();
                
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messages = System.Text.Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received message: {Message}", messages);
                    var message = JsonSerializer.Deserialize<OrdersModel>(body);
                    _logger.LogInformation("Received message: {Message}", message);
                    // Process the message here
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken); // Keep the service running
                }
            }

        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // This method will be called by the host to stop the background service.
            Console.WriteLine("Stopping RabbitMQ message processing...");
            return base.StopAsync(cancellationToken);
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // This method will be called by the host to start the background service.
            Console.WriteLine("Starting RabbitMQ message processing...");
            return base.StartAsync(cancellationToken);
        }
    }
}
