using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _coll;
    private readonly IProducer<Null, string> _producer;

    public OrderRepository(IMongoClient mongoClient, IProducer<Null, string> producer)
    {
        var mongoDatabase = mongoClient.GetDatabase("devoxx");
        _coll = mongoDatabase.GetCollection<Order>("orders");
        _producer = producer;
    }

    public async Task CreateAsync(Order newOrder)
    {
        await _coll.InsertOneAsync(newOrder);
        await _producer.ProduceAsync(
            "my-topic", 
            new Message<Null, string> { Value=$"Order {newOrder.Id} created!" }
        );
        _producer.Flush();
    }
}