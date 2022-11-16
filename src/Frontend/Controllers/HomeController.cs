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
        var toppings = await GetToppingsAsync();

        var crusts = await GetCrustsAsync();
        
        var viewModel = new HomeViewModel(toppings, crusts);
        return View(viewModel);
    }

    private async Task<List<ToppingViewModel>> GetToppingsAsync()
    {
        var response = await _ingredients.GetToppingsAsync(new GetToppingsRequest());

        var toppings = response.Toppings.Select(t => new ToppingViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Price = Convert.ToDecimal(t.Price),
            })
            .ToList();
        return toppings;
    }

    private async Task<List<CrustViewModel>> GetCrustsAsync()
    {
        var response = await _ingredients.GetCrustsAsync(new GetCrustsRequest());

        var crusts = response.Crusts.Select(t => new CrustViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Size = t.Size,
                Price = Convert.ToDecimal(t.Price),
            })
            .ToList();
        return crusts;
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