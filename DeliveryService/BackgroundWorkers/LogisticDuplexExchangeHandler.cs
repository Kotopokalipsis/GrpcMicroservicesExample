using Grpc.Core;
using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class LogisticDuplexExchangeHandler : IDisposable
{
    private AsyncDuplexStreamingCall<Request, Response> StreamExchange { get; set; } = null;

    private bool _disposedValue = false;

    public delegate void ReceiveMessageHandler(Response response);
    public event ReceiveMessageHandler? OnMessageReceived;

    private readonly SemaphoreSlim _locker = new(1);
    
    public void SetStream(AsyncDuplexStreamingCall<Request, Response> streamExchange)
    {
        StreamExchange = streamExchange;
    }
    
    public async Task WriteAsync(Request request, CancellationToken ct = default)
    {
        if (StreamExchange == null) return;
        
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
        if (StreamExchange == null) return;
        while (await StreamExchange.ResponseStream.MoveNext(ct))
        {
            var response = StreamExchange.ResponseStream.Current;
            OnMessageReceived?.Invoke(response);
        }
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
    
    ~LogisticDuplexExchangeHandler()
    {
        Dispose(false);
    }
}