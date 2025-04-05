using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;

public static class Helper
{
    private static Random random = new Random();

    public static Order GenerateOrder()
    {
        return new Order
        {
            Id = ObjectId.GenerateNewId(),
            Price = decimal.Parse($"{random.Next(1,1000)},{random.Next(10, 99)}"),
            Ean = RandomString(13),
            Quantity = random.Next(1, 100),
            Status = "PENDING",
            DeliveryDate = DateTime.UtcNow
        };
    }

    public static OutboxMessage ToOutbox(this Order order)
    {
        return new OutboxMessage
        {
            Id = ObjectId.GenerateNewId(),
            Content = JsonSerializer.Serialize(order),
            OccurredOnUtc = DateTime.UtcNow,
            Type = "NEW_ORDER"
        };
    }

    public static int NextInt32(this Random rng)
    {
        int firstBits = rng.Next(0, 1 << 4) << 28;
        int lastBits = rng.Next(0, 1 << 28);
        return firstBits | lastBits;
    }

    public static decimal NextDecimal(this Random rng)
    {
        byte scale = (byte)rng.Next(29);
        bool sign = rng.Next(2) == 1;
        return new decimal(rng.NextInt32(),
                           rng.NextInt32(),
                           rng.NextInt32(),
                           sign,
                           scale);
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}