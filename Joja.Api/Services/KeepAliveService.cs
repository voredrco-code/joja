using Microsoft.Extensions.Hosting;

namespace Joja.Api.Services;

public class KeepAliveService : BackgroundService
{
    private readonly ILogger<KeepAliveService> _logger;
    private readonly HttpClient _httpClient;
    // Replace with your actual domain if it changes, but jojaskincare.com is currently used
    private readonly string _url = "https://jojaskincare.com/health"; 

    public KeepAliveService(ILogger<KeepAliveService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a few minutes after startup before the first ping
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Ping the external URL of this app so the request goes through Render's load balancer
                // This resets the 15-minute inactivity timer reliably, since it runs inside the app itself
                // and isn't subject to GitHub Actions queue delays.
                var response = await _httpClient.GetAsync(_url, stoppingToken);
                _logger.LogInformation($"[KeepAlive] Self-ping to {_url} returned: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[KeepAlive] Self-ping failed: {ex.Message}");
            }

            // Ping every 10 minutes (Render sleeps after 15 minutes of inactivity)
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
