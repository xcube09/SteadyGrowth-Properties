using SteadyGrowth.Web.Services.Interfaces;

namespace SteadyGrowth.Web.BackgroundServices
{
    /// <summary>
    /// Background service that periodically checks and corrects KYC statuses for all users
    /// </summary>
    public class KycStatusCorrectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KycStatusCorrectionService> _logger;
        private readonly IConfiguration _configuration;

        // Default check interval - can be overridden by configuration
        private readonly TimeSpan _defaultCheckInterval = TimeSpan.FromHours(1);

        public KycStatusCorrectionService(
            IServiceProvider serviceProvider,
            ILogger<KycStatusCorrectionService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check if the service is enabled
            var isEnabled = _configuration.GetValue<bool>("KycStatusCorrection:Enabled", true);
            if (!isEnabled)
            {
                _logger.LogInformation("KYC Status Correction Service is disabled via configuration");
                return;
            }

            // Get check interval from configuration
            var intervalHours = _configuration.GetValue<int>("KycStatusCorrection:IntervalInHours", 1);
            var checkInterval = TimeSpan.FromHours(intervalHours);

            _logger.LogInformation("KYC Status Correction Service started with {IntervalHours} hour interval", intervalHours);

            // Check if we should run on startup
            var runOnStartup = _configuration.GetValue<bool>("KycStatusCorrection:RunOnStartup", true);
            if (runOnStartup)
            {
                _logger.LogInformation("Running initial KYC status correction on startup");
                await PerformKycStatusCorrection(stoppingToken);
            }

            // Start the periodic check loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(checkInterval, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformKycStatusCorrection(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("KYC Status Correction Service is stopping due to cancellation");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in KYC Status Correction Service main loop");

                    // Wait a bit before retrying to avoid rapid repeated failures
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("KYC Status Correction Service stopped");
        }

        private async Task PerformKycStatusCorrection(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting KYC status correction cycle");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var kycStatusService = scope.ServiceProvider.GetRequiredService<IKycStatusService>();

                    // Check if cancellation was requested before starting the work
                    cancellationToken.ThrowIfCancellationRequested();

                    await kycStatusService.CorrectAllUserKycStatusesAsync();

                    _logger.LogDebug("KYC status correction cycle completed successfully");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("KYC status correction was cancelled");
                throw; // Re-throw to handle at higher level
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing KYC status correction");
                // Don't re-throw here to allow the service to continue running
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KYC Status Correction Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}