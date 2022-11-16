using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using Ingredients.Protos;

namespace Frontend.Controllers;

public class HomeController : Controller
{
    private readonly IngredientsService.IngredientsServiceClient _ingredients;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IngredientsService.IngredientsServiceClient ingredients, ILogger<HomeController> logger)
    {
        _ingredients = ingredients;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _ingredients.GetToppingsAsync(new GetToppingsRequest());

        var toppings = response.Toppings.Select(t => new ToppingViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Price = Convert.ToDecimal(t.Price),
            })
            .ToList();

        var crusts = new List<CrustViewModel>
        {
            new("thin9", "Thin", 9, 5m),
            new("deep9", "Deep", 9, 6m),
        };
        var viewModel = new HomeViewModel(toppings, crusts);
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}