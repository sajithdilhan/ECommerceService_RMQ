namespace Shared.Common;

public static class Constants
{
    public const string AuthenticationSectionName = "Authentication";
    public const string ApiKeyHeaderName = "X-API-KEY";
    public const string ApiKeyName = "ApiKey";

    public const string CacheKeyUserPrefix = "User_";
    public const string CacheKeyOrderPrefix = "Order_";

    public const string KafkaBootstrapServersConfigKey = "Kafka:BootstrapServers";
    public const string KafkaConsumerGroupIdConfigKey = "Kafka:GroupId";
    public const string KafkaConsumerTopicConfigKey = "Kafka:ConsumerTopic";
    public const string KafkaProducerTopicConfigKey = "Kafka:ProducerTopic";
}
