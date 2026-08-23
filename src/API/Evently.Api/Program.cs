using Evently.Api.Extensions;
using Evently.Common.Application;
using Evently.Common.Infrastructure;
using Evently.Modules.Events.Infrastructure;
using Serilog;

namespace Evently.Api;

internal static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));
        
        builder.Services.AddOpenApi();

        builder.Configuration.AddModuleConfiguration(["events"]);
        
        builder.Services.AddApplication([Modules.Events.Application.AssemblyReference.Assembly]);
        
        builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Database")!);
        
        builder.Services.AddEventsModule(builder.Configuration);
        
        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.ApplyMigrations();
            app.MapOpenApi();
        }
        
        EventsModule.MapEndpoints(app);

        app.UseSerilogRequestLogging();

        app.Run();
    }
}
