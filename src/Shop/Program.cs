using Orders.Protos;
using Shop;

var macOS = OperatingSystem.IsMacOS();
var binding = macOS ? "http" : "https";
var defaultUri = macOS ? "http://localhost:5004" : "https://localhost:5005";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddGrpcClient<OrderService.OrderServiceClient>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var ordersUri = configuration.GetServiceUri("orders", binding)
                            ?? new Uri(defaultUri);
            options.Address = ordersUri;
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
