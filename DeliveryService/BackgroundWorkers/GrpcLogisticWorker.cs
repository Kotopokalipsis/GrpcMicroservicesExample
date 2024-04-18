using Grpc.Core;
using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class GrpcLogisticWorker : BackgroundService
{
    private readonly LogisticApiStreamClient _logisticApiStreamClient;

    public GrpcLogisticWorker(LogisticApiStreamClient logisticApiStreamClient)
    {
        _logisticApiStreamClient = logisticApiStreamClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logisticApiStreamClient.OnMessageReceived += _exchange_OnMessageReceived;
                await _logisticApiStreamClient.ReadingAsync(stoppingToken);
            }
            catch (RpcException e)
            {
                if (e.StatusCode != StatusCode.Unavailable) throw;
                
                if (!stoppingToken.IsCancellationRequested) await Task.Delay(10_000, stoppingToken);
                continue;
            }
        }
    }

    private void _exchange_OnMessageReceived(Response response)
    {
        Console.WriteLine($"Received response from logistic service: CorrelationId = {response.CorrelationId}");
    }
}