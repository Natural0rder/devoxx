using MongoDB.Driver;

public class OrderWithTransactionOutboxRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _coll;
    private readonly IMongoCollection<OutboxMessage> _collOutbox;
    private readonly IMongoClient _mongoClient;

    public OrderWithTransactionOutboxRepository(IMongoClient mongoClient)
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
                    await _coll.InsertOneAsync(newOrder); // Could be any write op
                    await _collOutbox.InsertOneAsync(newOrder.ToOutbox()); // Define a proper outbox document
                    return string.Empty;
                }, cancellationToken: cts.Token);
        }
    }
}