using System.Text.Json;
using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;

public class OutboxProcessor
{
    private const int BatchSize = 1000;
    private readonly IMongoCollection<OutboxMessage> _coll;
    private readonly IProducer<Null, string> _producer;
    private readonly IMongoClient _mongoClient;
    
    public OutboxProcessor(IMongoClient mongoClient, 
                           IProducer<Null, string> producer)
    {
       var mongoDatabase = mongoClient.GetDatabase("devoxx");
        _coll = mongoDatabase.GetCollection<OutboxMessage>("ordersOutbox");
        _producer = producer; 
        _mongoClient = mongoClient;
    }

    public async Task<long> Execute(CancellationToken cancellationToken = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Exists(u => u.ProcessedOnUtc, false);
        var sort = Builders<OutboxMessage>.Sort.Ascending(u => u.OccurredOnUtc);
        var projection = Builders<OutboxMessage>.Projection
            .Include(u => u.Content)
            .Include(u => u.Type)
            .Include(u => u.Id);
      
        var messages = await _coll.Find(filter)
                                  .Sort(sort)
                                  .Project<OutboxMessage>(projection)
                                  .Limit(BatchSize)
                                  .ToListAsync();

        var publishTasks = messages
            .Select(message => PublishMessage(message, _producer, cancellationToken))
            .ToList();
        var published = await Task.WhenAll(publishTasks);
        var modifiedCount = await BulkUpdateProcessedOnAsync(_mongoClient, _coll, published.ToList());

        return modifiedCount;
    }

    private static async Task<long> BulkUpdateProcessedOnAsync(
        IMongoClient mongoClient,
        IMongoCollection<OutboxMessage> coll,
        List<(ObjectId id, DateTime processedOn)> updates)
    {
        using var session = await mongoClient.StartSessionAsync();

        var modifiedCount = await session.WithTransactionAsync(async (s, ct) =>
        {
            var bulkOps = new List<WriteModel<OutboxMessage>>();

            foreach (var (id, processedOn) in updates)
            {
                var filter = Builders<OutboxMessage>.Filter.Eq(u => u.Id, id);
                var update = Builders<OutboxMessage>.Update.Set(u => u.ProcessedOnUtc, processedOn);
                var updateOne = new UpdateOneModel<OutboxMessage>(filter, update) { IsUpsert = false };
                bulkOps.Add(updateOne);
            }

            if (bulkOps.Count > 0)
            {
                var result = await coll.BulkWriteAsync(s, bulkOps);
                return result.ModifiedCount;
            }

            return 0;
        });

        return modifiedCount;
    }

    private static async Task<(ObjectId Id, DateTime PublishedOn)> PublishMessage(
        OutboxMessage message,
        IProducer<Null, string> producer,
        CancellationToken cancellationToken)
    {
            var deserializedMessage = JsonSerializer.Deserialize(message.Content, typeof(OutboxMessage));
            await producer.ProduceAsync(
                "my-topic", 
                new Message<Null, string> { Value=$"{deserializedMessage}" },
                cancellationToken
            );
            producer.Flush();
            return (message.Id, DateTime.UtcNow);
    }
}