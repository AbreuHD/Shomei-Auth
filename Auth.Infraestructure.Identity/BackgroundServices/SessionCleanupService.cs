using Auth.Infraestructure.Identity.Features.UserSessions.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Auth.Infraestructure.Identity.BackgroundServices
{
    public class SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting cleaning of expired sessions...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new DeleteExpiredSessionsCommand(), stoppingToken);
                    _logger.LogInformation("Cleaning of expired sessions completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing sessions");
                }
                await Task.Delay(TimeSpan.FromHours(5), stoppingToken);
            }
        }
    }
}
