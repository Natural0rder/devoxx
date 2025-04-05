using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;

public class OrderWithTransactionRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _coll;
    private readonly IProducer<Null, string> _producer;
    private readonly IMongoClient _mongoClient;

    public OrderWithTransactionRepository(IMongoClient mongoClient, IProducer<Null, string> producer)
    {
        var mongoDatabase = mongoClient.GetDatabase("devoxx");
        _coll = mongoDatabase.GetCollection<Order>("orders");
        _producer = producer;
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
                    await _producer.ProduceAsync(
                        "my-topic",
                        new Message<Null, string> { Value = $"Order {newOrder.Id} created!" }
                    );
                    _producer.Flush();

                    return string.Empty;
                }, cancellationToken: cts.Token);
        }
    }
}