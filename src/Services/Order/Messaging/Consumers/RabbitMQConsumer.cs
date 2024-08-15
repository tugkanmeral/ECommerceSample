using RabbitMQ.Client;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private IConnection _connection;
    private IModel _channel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var hostName = _configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:HostName");
        var port = _configuration.GetValue<int>("RabbitMQSettings:ConnectionInfo:Port");
        var username = _configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:Username");
        var password = _configuration.GetValue<string>("RabbitMQSettings:ConnectionInfo:Password");

        ArgumentNullException.ThrowIfNull(hostName);
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        ConnectionFactory _connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            Port = port,
            UserName = username,
            Password = password
        };

        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        ApproveQueueConsumer.Run(_channel, _serviceProvider, _configuration, _logger, cancellationToken);
        CancelQueueConsumer.Run(_channel, _serviceProvider, _configuration, _logger, cancellationToken);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}