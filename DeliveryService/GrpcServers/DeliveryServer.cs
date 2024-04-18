using AutoMapper;
using DeliveryServer.BackgroundWorkers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LogisticService;

namespace DeliveryServer.GrpcServers;

public class DeliveryServer : Delivery.DeliveryBase
{
    private readonly LogisticApiStreamClient _logisticApiStreamClient;

    public DeliveryServer(LogisticApiStreamClient logisticApiStreamClient)
    {
        _logisticApiStreamClient = logisticApiStreamClient;
    }

    public override Task<DeliveryResponse> StartDelivery(Order order, ServerCallContext context)
    {
        Console.WriteLine($"Handel request, OrderId = {order.Id}, OrderStatus = {order.Status}");
        
        // Changing status
        order.Status = OrderStatus.Shipping;
        
        Console.WriteLine($"Changed status on Shipping and delivery is processing");

        var tasks = new List<Task>();
        
        // Simulating multiple threads
        foreach (var item in order.Items)
        {
            var task = Task.Run(async () =>
            {
                Console.WriteLine($"Start task for item: Id = {item.Id}, Name = {item.Name}");
                
                await _logisticApiStreamClient.WriteAsync(new Request()
                {
                    MessageId = $"{Guid.NewGuid()}",
                    Item = item,
                    OrderId = order.Id,
                    PingMessage = new Empty(),
                    Type = RequestType.StartShipping,
                });
            });
            
            tasks.Add(task);
        }
        
        // Return response to Orders service
        return Task.FromResult(new DeliveryResponse()
        {
            Order = order,
            Status = DeliveryRequestStatus.Processing,
        });
    }
}