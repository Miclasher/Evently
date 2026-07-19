using Evently.Modules.Events.Api.Database;
using Evently.Modules.Events.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        
        ApplyMigration<EventsDbContext>(scope);
    }

    private static void ApplyMigration<TContext>(IServiceScope scope)
        where TContext : DbContext
    {
        using TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            
        dbContext.Database.Migrate();
    }
}
