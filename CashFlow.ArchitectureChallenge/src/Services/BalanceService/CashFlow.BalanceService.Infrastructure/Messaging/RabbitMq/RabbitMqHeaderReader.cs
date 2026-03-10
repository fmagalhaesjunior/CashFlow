using RabbitMQ.Client;

namespace CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;

public static class RabbitMqHeaderReader
{
    public static int GetConsumerRetryCount(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers is null)
        {
            return 0;
        }

        return !properties.Headers.TryGetValue("consumer_retry_count", out var value) || value is null
            ? 0
            : value switch
            {
                byte[] bytes when int.TryParse(System.Text.Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
                int i => i,
                long l => (int)l,
                string s when int.TryParse(s, out var parsed) => parsed,
                _ => 0
            };
    }
}