public interface IOrderRepository
{
    Task CreateAsync(Order newOrder);
}