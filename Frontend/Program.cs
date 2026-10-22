var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Provide configuration to frontend JS
app.MapGet("/api/config", (IConfiguration config) =>
{
    var apiUrl = config["BackendApi:BaseUrl"] ?? "http://localhost:5000";
    return Results.Ok(new { apiUrl });
});

app.Run();
