namespace ReconciliationDashboard.Api.Events;

public sealed class LoggingEventDispatcher : IEventDispatcher
{
    private readonly ILogger<LoggingEventDispatcher> _logger;

    public LoggingEventDispatcher(ILogger<LoggingEventDispatcher> logger) => _logger = logger;

    public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        if (@event is AccountFlaggedEvent e)
        {
            _logger.LogWarning(
                "[ACCOUNT_FLAGGED] AccountId={AccountId} AccountNumber={AccountNumber} Customer={CustomerName} " +
                "Status={Status} Reason={FlagReason} At={OccurredAt:O}",
                e.AccountId, e.AccountNumber, e.CustomerName, e.Status, e.FlagReason ?? "none", e.OccurredAt);
        }
        else
        {
            _logger.LogInformation("[EVENT] {EventType} dispatched at {At:O}", typeof(TEvent).Name, DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }
}
