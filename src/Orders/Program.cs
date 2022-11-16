using Ingredients.Protos;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

var ingredientsAddress = OperatingSystem.IsMacOS()
    ? "http://localhost:5002"
    : "https://localhost:5003";

var binding = OperatingSystem.IsMacOS() ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding) ?? new Uri(ingredientsAddress);

AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddGrpc();
builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

var app = builder.Build();

app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Hello World!");

app.Run();
