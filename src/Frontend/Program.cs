using Ingredients.Protos;

var builder = WebApplication.CreateBuilder(args);

var ingredientsAddress = OperatingSystem.IsMacOS()
    ? "http://localhost:5002"
    : "https://localhost:5003";

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = new Uri(ingredientsAddress);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
