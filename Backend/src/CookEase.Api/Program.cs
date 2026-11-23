using CookEase.Infrastructure;
using CookEase.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CookEase")
    ?? throw new InvalidOperationException(
        "Set ConnectionStrings__CookEase before starting the API.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddInfrastructureServices();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { name = "CookEase API" }))
    .ExcludeFromDescription();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program { }
