using DeliveryServer.BackgroundWorkers;
using LogisticService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LogisticApiStreamClient>();
builder.Services.AddHostedService<GrpcLogisticWorker>();

builder.Services.AddGrpc();
builder.Services.AddGrpcClient<Logistic.LogisticClient>(o =>
{
    o.Address = new Uri("https://localhost:7021");
});

var app = builder.Build();

app.MapGrpcService<DeliveryServer.GrpcServers.DeliveryServer>();

app.Run();