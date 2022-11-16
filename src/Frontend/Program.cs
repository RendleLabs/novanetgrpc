using Ingredients.Protos;

var builder = WebApplication.CreateBuilder(args);

var ingredientsAddress = OperatingSystem.IsMacOS()
    ? "http://localhost:5002"
    : "https://localhost:5003";

var binding = OperatingSystem.IsMacOS() ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding) ?? new Uri(ingredientsAddress);

AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
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
