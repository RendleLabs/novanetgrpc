using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;

namespace Ingredients.Services;

public class IngredientsImpl : Protos.IngredientsService.IngredientsServiceBase
{
    private readonly IToppingData _toppingData;
    private readonly ICrustData _crustData;
    private readonly ILogger<IngredientsImpl> _logger;

    public IngredientsImpl(IToppingData toppingData, ICrustData crustData, ILogger<IngredientsImpl> logger)
    {
        _toppingData = toppingData;
        _crustData = crustData;
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

    public override async Task<GetCrustsResponse> GetCrusts(GetCrustsRequest request, ServerCallContext context)
    {
        try
        {
            var crusts = await _crustData.GetAsync(context.CancellationToken);

            var response = new GetCrustsResponse
            {
                Crusts =
                {
                    crusts.Select(c => new Crust
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Size = c.Size,
                        Price = c.Price,
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

    public override async Task<DecrementToppingsResponse> DecrementToppings(DecrementToppingsRequest request, ServerCallContext context)
    {
        foreach (var toppingId in request.ToppingIds)
        {
            await _toppingData.DecrementStockAsync(toppingId, context.CancellationToken);
        }

        return _decrementToppingsResponse;
    }

    private static readonly DecrementToppingsResponse _decrementToppingsResponse = new();

    public override async Task<DecrementCrustsResponse> DecrementCrusts(DecrementCrustsRequest request, ServerCallContext context)
    {
        foreach (var crustId in request.CrustIds)
        {
            await _crustData.DecrementStockAsync(crustId, context.CancellationToken);
        }

        return new DecrementCrustsResponse();
    }
}