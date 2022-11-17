using Grpc.Core;
using Orders.Protos;

namespace Shop;

public class Worker : BackgroundService
{
    private readonly OrderService.OrderServiceClient _client;
    private readonly ILogger<Worker> _logger;
    private readonly HashSet<string> _seen = new();

    public Worker(OrderService.OrderServiceClient client, ILogger<Worker> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stream = _client.Subscribe(new SubscribeRequest());

                await foreach (var notification in stream.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    if (_seen.Add(notification.NotificationId))
                    {
                        _logger.LogInformation("Order {CrustId} with {ToppingIds} due by {DueBy}",
                            notification.CrustId,
                            string.Join(", ", notification.ToppingIds),
                            notification.DueBy.ToDateTimeOffset());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
