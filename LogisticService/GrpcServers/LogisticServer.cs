using Grpc.Core;

namespace LogisticService.GrpcServers;

public class LogisticServer : Logistic.LogisticBase
{
    public override async Task StreamData(IAsyncStreamReader<Request> request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        await foreach (var message in request.ReadAllAsync())
        {
            await responseStream.WriteAsync(new Response() {CorrelationId = message.MessageId});
        }
    }
}