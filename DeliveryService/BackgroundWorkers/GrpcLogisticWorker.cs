using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class GrpcLogisticWorker : BackgroundService
{
    private readonly LogisticApiStreamClient _logisticApiStreamClient;
    private readonly ILogger<GrpcLogisticWorker> _logger;

    public GrpcLogisticWorker(LogisticApiStreamClient logisticApiStreamClient, ILogger<GrpcLogisticWorker> logger)
    {
        _logisticApiStreamClient = logisticApiStreamClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logisticApiStreamClient.OnMessageReceived += _exchange_OnMessageReceived;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _logisticApiStreamClient.ReadingAsync(stoppingToken);
            }
            catch (Exception e)
            {
                await Task.Delay(10_000, stoppingToken);
                _logger.LogCritical(e.Message, e);
            }
        }
    }

    private void _exchange_OnMessageReceived(Response response)
    {
        Console.WriteLine("ПРИВЕЕЕЕЕЕЕЕЕЕЕТ");
        Console.WriteLine($"Received response from logistic service: CorrelationId = {response.CorrelationId}");
    }
}