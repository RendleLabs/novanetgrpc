using Grpc.Health.V1;
using Grpc.HealthCheck;
using Ingredients.Data;

namespace Ingredients;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly HealthServiceImpl _healthServiceImpl;
    private readonly IToppingData _toppingData;

    public HealthCheckBackgroundService(HealthServiceImpl healthServiceImpl, IToppingData toppingData)
    {
        _healthServiceImpl = healthServiceImpl;
        _toppingData = toppingData;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await _toppingData.GetAsync(stoppingToken);
                    _healthServiceImpl.SetStatus("ingredients", HealthCheckResponse.Types.ServingStatus.Serving);
                }
                catch
                {
                    _healthServiceImpl.SetStatus("ingredients", HealthCheckResponse.Types.ServingStatus.NotServing);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}