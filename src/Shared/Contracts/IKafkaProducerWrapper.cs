namespace Shared.Contracts;

public interface IKafkaProducerWrapper
{
    Task ProduceAsync<T>(Guid key, T eventObject, CancellationToken token) where T : class;
}
