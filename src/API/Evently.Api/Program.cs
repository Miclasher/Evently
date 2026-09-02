using Evently.Api.Extensions;
using Evently.Api.Middleware;
using Evently.Common.Application;
using Evently.Common.Infrastructure;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Events.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Evently.Api;

internal static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        
        builder.Services.AddProblemDetails();
        
        builder.Services.AddOpenApi();

        builder.Configuration.AddModuleConfiguration(["events"]);
        
        builder.Services.AddApplication([Modules.Events.Application.AssemblyReference.Assembly]);

        string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
        string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;
        builder.Services.AddInfrastructure(
            databaseConnectionString,
            redisConnectionString);

        builder.Services.AddHealthChecks()
            .AddNpgSql(databaseConnectionString)
            .AddRedis(redisConnectionString);
        
        builder.Services.AddEventsModule(builder.Configuration);
        
        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.ApplyMigrations();
            app.MapOpenApi();
        }

        app.MapEndpoints();
        
        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();
        
        app.Run();
    }
}
