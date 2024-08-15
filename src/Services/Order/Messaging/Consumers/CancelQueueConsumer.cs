using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class CancelQueueConsumer
{
    public static void Run(IModel channel, IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger, CancellationToken cancellationToken)
    {
        var queueName = configuration.GetValue<string>("RabbitMQSettings:QueueNames:OrderCancel");
        var errorQueueueueName = configuration.GetValue<string>("RabbitMQSettings:QueueNames:ErrorOrderCancel");
        var maxRetryCount = configuration.GetValue<int>("RabbitMQSettings:RetryCounts:OrderCancel");

        var orderCancelQueueConsumer = new EventingBasicConsumer(channel);
        orderCancelQueueConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var headers = ea.BasicProperties.Headers;

            var order = JsonConvert.DeserializeObject<OrderDTO>(message);
            ArgumentNullException.ThrowIfNull(order);

            int retryCount = 1;
            if (headers != null && headers.ContainsKey("retry-count"))
                retryCount = Convert.ToInt32(Encoding.UTF8.GetString((byte[])headers["retry-count"]));

            try
            {
                throw new Exception("TUGKAN EXCEPTION HERE!");

                using (var scope = serviceProvider.CreateScope())
                {
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    await orderService.CancelOrder(order!, cancellationToken);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                if (retryCount >= maxRetryCount)
                {
                    logger.LogInformation($"'{message}' sending to {errorQueueueueName}");

                    channel.BasicPublish(
                    exchange: "",
                    routingKey: errorQueueueueName,
                    basicProperties: null,
                    body: body);

                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
                else
                {
                    retryCount++;
                    var properties = channel.CreateBasicProperties();
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "retry-count", Encoding.UTF8.GetBytes(retryCount.ToString()) }
                    };

                    logger.LogInformation($"'{message}' sending to {queueName}. Retry count is {retryCount}");

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queueName,
                        basicProperties: properties,
                        body: body);

                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
        };

        channel.QueueDeclare(queue: errorQueueueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: orderCancelQueueConsumer);

        logger.LogInformation($"Listening on queue: {queueName}");
    }
}