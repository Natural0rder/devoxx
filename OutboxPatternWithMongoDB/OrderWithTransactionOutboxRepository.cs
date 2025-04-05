using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;

public class OrderWithTransactionOutboxRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _coll;
    private readonly IMongoCollection<OutboxMessage> _collOutbox;
    private readonly IMongoClient _mongoClient;

    public OrderWithTransactionOutboxRepository(IMongoClient mongoClient, IProducer<Null, string> producer)
    {
        var mongoDatabase = mongoClient.GetDatabase("devoxx");
        _coll = mongoDatabase.GetCollection<Order>("orders");
        _collOutbox = mongoDatabase.GetCollection<OutboxMessage>("ordersOutbox");
        _mongoClient = mongoClient;
    }

    public async Task CreateAsync(Order newOrder)
    {
        using (var session = await _mongoClient.StartSessionAsync())
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
        {
            await session.WithTransactionAsync(
                async (s, ct) =>
                {
                    await _coll.InsertOneAsync(newOrder);
                    await _collOutbox.InsertOneAsync(newOrder.ToOutbox());
                    return string.Empty;
                }, cancellationToken: cts.Token);
        }
    }
}