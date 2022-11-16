using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;

namespace Ingredients.Services;

public class IngredientsImpl : Protos.IngredientsService.IngredientsServiceBase
{
    private readonly IToppingData _toppingData;
    private readonly ILogger<IngredientsImpl> _logger;

    public IngredientsImpl(IToppingData toppingData, ILogger<IngredientsImpl> logger)
    {
        _toppingData = toppingData;
        _logger = logger;
    }

    public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
    {
        try
        {
            var toppings = await _toppingData.GetAsync(context.CancellationToken);

            var response = new GetToppingsResponse
            {
                Toppings =
                {
                    toppings.OrderBy(t => t.Id)
                        .Select(t => new Topping
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Price = t.Price,
                        })
                }
            };

            return response;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "GetToppings cancelled");
            throw;
        }
    }
}