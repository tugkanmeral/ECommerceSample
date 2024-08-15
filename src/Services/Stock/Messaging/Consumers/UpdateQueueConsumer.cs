using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class UpdateQueueConsumer
{
    public static void Run(IModel channel, IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger, CancellationToken cancellationToken)
    {
        var queueName = configuration.GetValue<string>("RabbitMQSettings:QueueNames:StockUpdate");

        var stockUpdateQueueConsumer = new EventingBasicConsumer(channel);
        stockUpdateQueueConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var order = JsonConvert.DeserializeObject<OrderDTO>(message);

            using (var scope = serviceProvider.CreateScope())
            {
                var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

                await stockService.UpdateStock(order!, cancellationToken);
            }
        };

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        channel.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: stockUpdateQueueConsumer);

        logger.LogInformation($"Listening on queue: {queueName}");
    }
}