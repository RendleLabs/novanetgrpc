using System.Security.Claims;
using AuthHelp;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orders.PubSub;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

var ingredientsAddress = OperatingSystem.IsMacOS()
    ? "http://localhost:5002"
    : "https://localhost:5003";

var binding = OperatingSystem.IsMacOS() ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding) ?? new Uri(ingredientsAddress);

AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddGrpc();
builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

builder.Services.AddOrderPubSub();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor = false,
            ValidateLifetime = false,
            IssuerSigningKey = JwtHelper.SecurityKey,
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(JwtBearerDefaults.AuthenticationScheme, p =>
    {
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        p.RequireClaim(ClaimTypes.Name);
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Hello World!");

app.Run();
