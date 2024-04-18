using Grpc.Core;
using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class LogisticApiStreamClient : IDisposable
{
    public delegate void ReceiveMessageHandler(Response response);
    public event ReceiveMessageHandler? OnMessageReceived;

    private bool _disposedValue = false;
    private readonly SemaphoreSlim _locker = new(1);
    
    private AsyncDuplexStreamingCall<Request, Response> StreamExchange { get; set; } = null;
    
    private readonly Logistic.LogisticClient _logisticClient;

    public LogisticApiStreamClient(Logistic.LogisticClient logisticClient)
    {
        _logisticClient = logisticClient;
    }

    public async Task WriteAsync(Request request, CancellationToken ct = default)
    {
        CheckStream(ct);
        
        await _locker.WaitAsync(ct);
        
        try
        {
            await StreamExchange.RequestStream.WriteAsync(request, ct);
        }
        finally
        {
            _locker.Release();
        }
    }

    public async Task ReadingAsync(CancellationToken ct)
    {
        CheckStream(ct);
        
        while (await StreamExchange.ResponseStream.MoveNext(ct))
        {
            var response = StreamExchange.ResponseStream.Current;
            OnMessageReceived?.Invoke(response);
        }
    }
    
    private void CheckStream(CancellationToken ct)
    {
        if (StreamExchange != null) return;
        StreamExchange = _logisticClient.StreamData(cancellationToken: ct);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                StreamExchange?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    
    ~LogisticApiStreamClient()
    {
        Dispose(false);
    }
}