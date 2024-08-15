using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

public class RabbitMQProducer : IMessageProducer
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQProducer> _logger;

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
    {
        var hostName = configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:HostName");
        var port = configuration.GetValue<int>("RabbitMQSettings:ConnectionInfo:Port");
        var username = configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:Username");
        var password = configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:Password");

        ArgumentNullException.ThrowIfNull(hostName);
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        _connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            Port = port,
            UserName = username,
            Password = password
        };

        _logger = logger;
    }

    public void SendMessage<T>(T message, string queueName)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);

        _logger.LogInformation($"Message sent by Order: {json}");

        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}