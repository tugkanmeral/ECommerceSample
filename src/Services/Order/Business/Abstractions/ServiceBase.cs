public abstract class ServiceBase(IConfiguration configuration)
{
    public string GetQueueName(string queueName)
    {
        var queueNameValue = configuration.GetValue<string>($"RabbitMQSettings:QueueNames:{queueName}");
        ArgumentNullException.ThrowIfNull(queueNameValue);
        return queueNameValue;
    }
}