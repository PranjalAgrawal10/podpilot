using PodPilot.Api.Extensions;
using PodPilot.Api.Middleware;
using PodPilot.Application;
using PodPilot.Infrastructure;
using PodPilot.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PodPilot.Api")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/podpilot-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInfrastructureHostedServices(builder.Environment);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPodPilotSwagger(builder.Configuration);

var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

if (!builder.Environment.IsEnvironment("Testing"))
{
    healthChecksBuilder.AddMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: ["db", "mysql"]);
}
else
{
    healthChecksBuilder.AddCheck(
        "database",
        () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("In-memory database"));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await DatabaseInitializer.InitializeAsync(app.Services);
}
else
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();

    var identityService = scope.ServiceProvider.GetRequiredService<PodPilot.Application.Common.Interfaces.IIdentityService>();
    await identityService.EnsureRolesExistAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]);
    };
});
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UsePodPilotSwagger();

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PodPilot.Infrastructure.Hubs.PodStatusHub>("/hubs/pods");
app.MapHub<PodPilot.Infrastructure.Hubs.GatewayHub>("/hubs/gateway");

try
{
    var urls = app.Urls.Count > 0
        ? string.Join(", ", app.Urls)
        : builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
    Log.Information("Starting PodPilot API at {Urls}", urls);
    Log.Information("Swagger UI: {SwaggerUrl}", urls.Split(';', ',')[0].TrimEnd('/') + "/swagger");
    await app.RunAsync();
}
catch (Exception ex) when (IsPortInUseException(ex))
{
    Log.Fatal(
        "Failed to start — port is already in use. Another PodPilot.Api instance may still be running. " +
        "Run: netstat -ano | findstr \":5000\" then taskkill /PID <pid> /F");
    Environment.ExitCode = 1;
}
catch (Exception ex)
{
    Log.Fatal(ex, "PodPilot API terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

static bool IsPortInUseException(Exception ex)
{
    for (var current = ex; current is not null; current = current.InnerException)
    {
        if (current is IOException && current.Message.Contains("address already in use", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (current is Microsoft.AspNetCore.Connections.AddressInUseException)
        {
            return true;
        }
    }

    return false;
}

/// <summary>
/// Partial Program class for integration test accessibility.
/// </summary>
public partial class Program
{
}
