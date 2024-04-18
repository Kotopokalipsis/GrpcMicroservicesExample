using Grpc.Core;
using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class GrpcLogisticWorker : BackgroundService
{
    private readonly Logistic.LogisticClient _logisticClient;
    private readonly LogisticDuplexExchangeHandler _exchangeHandler;

    public GrpcLogisticWorker(Logistic.LogisticClient logisticClient, LogisticDuplexExchangeHandler exchangeHandler)
    {
        _logisticClient = logisticClient;
        _exchangeHandler = exchangeHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _exchangeHandler.SetStream(_logisticClient.StreamData(cancellationToken: stoppingToken));
                _exchangeHandler.OnMessageReceived += _exchange_OnMessageReceived;
                await _exchangeHandler.ReadingAsync(stoppingToken);
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