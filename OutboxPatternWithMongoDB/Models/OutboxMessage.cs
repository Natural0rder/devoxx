using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public sealed class OutboxMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; init; }

    [BsonElement("type")]
    public required string Type { get; init; }

    [BsonElement("content")]
    public required string Content { get; init; }

    [BsonElement("occurredOnUtc")]
    public DateTime OccurredOnUtc { get; init; }

    [BsonElement("processedOnUtc")]
    public DateTime? ProcessedOnUtc { get; init; }

    [BsonElement("error")]
    public string? Error { get; init; }
}