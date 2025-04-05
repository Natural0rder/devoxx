using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Confluent.Kafka;
using MongoDB.Bson;

namespace Devoxx
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var app = serviceProvider.GetService<Application>();
            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOrderRepository, OrderWithTransactionRepository>();
            services.AddSingleton<Application>();

            services.AddSingleton<IMongoClient>(s =>
                new MongoClient("mongodb://localhost:27017")
            );

            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            services.AddSingleton(s => new ProducerBuilder<Null, string>(config).Build());
        }
    }

    public class Application
    {
        private readonly IOrderRepository _repository;

        public Application(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task RunAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                var order = Helper.GenerateOrder();
                await _repository.CreateAsync(order);
                Console.WriteLine($"Order {order.Id} created!");
            }
        }
    }
}