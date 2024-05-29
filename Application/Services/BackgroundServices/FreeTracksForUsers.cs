using Domain.Repositories;

namespace Application.Services.BackgroundServices;

public class FreeTracksForUsers : BackgroundService
{
    private readonly ILogger<FreeTracksForUsers> _logger;
    private readonly IServiceProvider _serviceProvider;

    public FreeTracksForUsers(
        ILogger<FreeTracksForUsers> logger,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Daily hosted service running.");
        var now = DateTimeOffset.Now;
        var timeUntilMidnight = TimeSpan.FromDays(1) - (now - now.Date);
        var nextExecutionTime = now + timeUntilMidnight;
        _logger.LogInformation("First execution scheduled at {NextExecutionTime}", nextExecutionTime);
        await Task.Delay(timeUntilMidnight, ct);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                _logger.LogInformation("Daily hosted service is work.");
                await using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    await userRepository.AddFreeTracksForAll(ct);
                }
                _logger.LogInformation("Daily hosted service is done.");
                var nextExecution = DateTimeOffset.Now.Date.AddDays(1);
                var delay = nextExecution - DateTimeOffset.Now;

                _logger.LogInformation("Waiting until next execution at {NextExecution}", nextExecution);
                await Task.Delay(delay, ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Daily Hosted Service stopped.");
        }
    }
}