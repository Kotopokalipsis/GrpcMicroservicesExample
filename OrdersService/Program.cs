using DeliveryServer;
using OrderServer.Client;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<Delivery.DeliveryClient>(o =>
{
    o.Address = new Uri("https://localhost:7022");
});

builder.Services.AddHostedService<OrderGenerator>();

var app = builder.Build();

app.Run();