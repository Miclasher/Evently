using System.Data;
using System.Data.Common;
using Dapper;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Data;
using Evently.Common.Domain;
using Evently.Common.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Evently.Modules.Users.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
    private const string ModuleName = "Users";
    
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        logger.LogInformation("{Module} - Beginning to process outbox messages", ModuleName);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        IReadOnlyCollection<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

        foreach (OutboxMessageResponse message in outboxMessages)
        {
            Exception? exception = null;
            
            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    message.Content,
                    SerializerSettings.Instance)!;
                
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                
                IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                
                await publisher.Publish(domainEvent, cancellationToken);
            }
            catch (Exception caughtException)
            {
                logger.LogError(caughtException, "{Module} - Error while processing outbox message {MessageId}", ModuleName, message.Id);
                
                exception = caughtException;
            }
            
            await UpdateOutboxMessageAsync(connection, transaction, message, exception);
        }
        
        
        await transaction.CommitAsync(cancellationToken);
        
        logger.LogInformation("{Module} - Finished processing outbox messages", ModuleName);
    }

    private async Task<IReadOnlyCollection<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        const string sql = $"""
                            SELECT
                                id AS {nameof(OutboxMessageResponse.Id)},
                                content AS {nameof(OutboxMessageResponse.Content)}
                            FROM {ModuleName}.outbox_messages
                            WHERE processed_on_utc IS NULL
                            ORDER BY occured_on_utc
                            LIMIT @BatchSize
                            FOR UPDATE SKIP LOCKED
                            """;

        IEnumerable<OutboxMessageResponse> outboxMessages =
            await connection.QueryAsync<OutboxMessageResponse>(
                sql, 
                new { outboxOptions.Value.BatchSize },
                transaction: transaction);

        return outboxMessages.ToList();
    }

    private async Task UpdateOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxMessageResponse outboxMessage,
        Exception? exception)
    {
        const string sql =
            $"""
            UPDATE {ModuleName}.outbox_messages
            SET processed_on_utc = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                outboxMessage.Id,
                ProcessedOnUtc = dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction);
    }

    
    internal sealed record OutboxMessageResponse(Guid Id, string Content);
}
