using DeliveryServer;
using Grpc.Core;
using Grpc.Net.Client;

namespace OrderServer.Client;

public class OrderGenerator : BackgroundService
{
    private readonly Delivery.DeliveryClient _client;

    public OrderGenerator(Delivery.DeliveryClient client)
    {
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var id = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = new Order()
            {
                Id = ++id,
                Status = OrderStatus.Created,
            };
            
            order.Items.Add(new Item()
            {
                Id = ++id,
                Name = "VERY BIG DISPLAY 25k"
            });
            
            order.Items.Add(new Item()
            {
                Id = ++id,
                Name = "IPHONE 666 XXL"
            });
            
            order.Items.Add(new Item()
            {
                Id = ++id,
                Name = "MY LITTLE PONY"
            });
            
            Console.WriteLine($"Send order, Id = {order.Id}, Status = {order.Status}");

            try
            {
                var result = await _client.StartDeliveryAsync(order, cancellationToken: stoppingToken);
                
                Console.WriteLine($"Result delivery, Delivery Status = {result.Status}, Error = {result.Error}, OrderId = {result.Order.Id}, OrderStatus = {result.Order.Status}");
            }
            catch (RpcException e)
            {
                if (e.StatusCode != StatusCode.Unavailable) throw;
                
                if (!stoppingToken.IsCancellationRequested) await Task.Delay(10_000, stoppingToken);
                id = id - 4; // One loop back
                continue;
            }

            Thread.Sleep(25000);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}