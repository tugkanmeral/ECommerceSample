using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client;

public class NotifyQueueConsumer
{
    public static void Run(IModel channel, IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger)
    {
        var queueName = configuration.GetValue<string>("RabbitMQSettings:QueueNames:Notify");

        var notificationNotifyQueueConsumer = new EventingBasicConsumer(channel);
        notificationNotifyQueueConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var notification = JsonConvert.DeserializeObject<Notification>(message);
            ArgumentNullException.ThrowIfNull(notification);

            using (var scope = serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                notificationService.Notify(notification);
            }

            logger.LogInformation($"Received message: {message}");
        };

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        channel.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: notificationNotifyQueueConsumer);

        logger.LogInformation($"Listening on queue: {queueName}");
    }
}