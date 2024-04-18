using Grpc.Core;
using LogisticService;

namespace DeliveryServer.BackgroundWorkers;

public class LogisticApiStreamClient : IDisposable
{
    public delegate void ReceiveMessageHandler(Response response);
    public event ReceiveMessageHandler? OnMessageReceived;

    private bool _disposedValue = false;
    private readonly SemaphoreSlim _locker = new(1);
    
    private AsyncDuplexStreamingCall<Request, Response> DuplexStream { get; set; } = null;
    
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
            await DuplexStream.RequestStream.WriteAsync(request, ct);
        }
        catch (RpcException e)
        {
            if (e.StatusCode == StatusCode.Unavailable)
            {
                DuplexStream = null;
            }
        }
        finally
        {
            _locker.Release();
        }
    }

    public async Task ReadingAsync(CancellationToken ct)
    {
        CheckStream(ct);

        try
        {
            while (await DuplexStream.ResponseStream.MoveNext(ct))
            {
                var response = DuplexStream.ResponseStream.Current;
                OnMessageReceived?.Invoke(response);
            }
        }
        catch (RpcException e)
        {
            if (e.StatusCode == StatusCode.Unavailable)
            {
                DuplexStream = null;
            }
        }
    }
    
    private void CheckStream(CancellationToken ct)
    {
        if (DuplexStream != null) return;
        DuplexStream = _logisticClient.StreamData(cancellationToken: ct);
    }

    public void Dispose()
    {
        DuplexStream?.Dispose();
    }
}