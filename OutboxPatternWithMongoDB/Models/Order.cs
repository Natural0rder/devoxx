using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("deliveryDate")]
        public DateTime DeliveryDate { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("ean")]
        public string? Ean { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }
    }