using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Evently.Modules.Users.Infrastructure.Outbox;

internal static class OutboxConfigurationExtensions
{
    public static void ConfigureOutboxJob(this IQuartzBuilder quartzBuilder)
    {
        string jobName = typeof(ProcessOutboxJob).FullName!;

        quartzBuilder
            .AddJob<ProcessOutboxJob>(configure => configure.WithIdentity(jobName));

        quartzBuilder
            .AddTrigger((sp, trigger) =>
            {
                OutboxOptions options = sp.GetRequiredService<IOptions<OutboxOptions>>().Value;

                trigger
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithInterval(TimeSpan.FromSeconds(options.IntervalInSeconds))
                            .RepeatForever());
            });
    }
}
