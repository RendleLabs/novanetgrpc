using Ingredients.Data;

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
}