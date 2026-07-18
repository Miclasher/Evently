using Evently.Api.Extensions;
using Evently.Modules.Events.Api.Events;

namespace Evently.Api;

internal static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        builder.Services.AddEventsModule(builder.Configuration);
        
        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.ApplyMigrations();
            app.MapOpenApi();
        }
        
        EventsModule.MapEndpoints(app);

        app.Run();
    }
}
