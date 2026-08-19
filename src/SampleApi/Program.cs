var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    Status = "Healthy",
    Message = "Welcome to Sample .NET 8 API deployed on GKE via Argo CD & GitHub Actions!",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
})
.WithName("GetRoot")
.WithOpenApi();

app.MapGet("/api/info", () => new
{
    AppName = "Sample.Gke.DotNetApi",
    Version = "1.0.0",
    Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
    MachineName = Environment.MachineName,
    Timestamp = DateTime.UtcNow
})
.WithName("GetInfo")
.WithOpenApi();

app.Run();
