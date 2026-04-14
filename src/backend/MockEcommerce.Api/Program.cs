var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<MockEcommerce.Api.Services.IProductService, MockEcommerce.Api.Services.MockProductService>();
builder.Services.AddSingleton<MockEcommerce.Api.Services.ICartService, MockEcommerce.Api.Services.InMemoryCartService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();

app.Run();

/// <summary>Enables <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/> in integration tests.</summary>
public partial class Program { }
