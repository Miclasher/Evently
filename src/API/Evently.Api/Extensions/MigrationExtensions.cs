using Evently.Modules.Attendance.Infrastructure.Database;
using Evently.Modules.Events.Infrastructure.Database;
using Evently.Modules.Ticketing.Infrastructure.Database;
using Evently.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        
        ApplyMigration<EventsDbContext>(scope);
        ApplyMigration<UsersDbContext>(scope);
        ApplyMigration<TicketingDbContext>(scope);
        ApplyMigration<AttendanceDbContext>(scope);
    }

    private static void ApplyMigration<TContext>(IServiceScope scope)
        where TContext : DbContext
    {
        using TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            
        dbContext.Database.Migrate();
    }
}
