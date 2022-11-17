using Ingredients.Protos;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Orders.PubSub;
using Orders.Services;

var runningInContainer = "true".Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));

var builder = WebApplication.CreateBuilder(args);

if (runningInContainer)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http2);
    });
}

var ingredientsAddress = OperatingSystem.IsMacOS()
    ? "http://localhost:5002"
    : "https://localhost:5003";

var binding = OperatingSystem.IsMacOS() || runningInContainer ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding) ?? new Uri(ingredientsAddress);

AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddGrpc();
builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

builder.Services.AddOrderPubSub();

var app = builder.Build();

app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Hello World!");

app.Run();
