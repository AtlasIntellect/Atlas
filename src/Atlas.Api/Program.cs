var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));

await app.RunAsync();