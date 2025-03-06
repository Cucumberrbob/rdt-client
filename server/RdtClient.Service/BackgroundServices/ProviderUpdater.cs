﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RdtClient.Data.Enums;
using RdtClient.Service.Services;

namespace RdtClient.Service.BackgroundServices;

public class ProviderUpdater(ILogger<TaskRunner> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private static DateTime _nextUpdate = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!Startup.Ready)
        {
            await Task.Delay(1000, stoppingToken);
        }

        using var scope = serviceProvider.CreateScope();
        var torrentService = scope.ServiceProvider.GetRequiredService<Torrents>();
            
        logger.LogInformation("ProviderUpdater started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var torrents = await torrentService.Get();
                
                if (_nextUpdate < DateTime.UtcNow && ((torrents.Count > 0 && !Settings.Get.Provider.AutoImport) || Settings.Get.Provider.AutoImport))
                {
                    logger.LogDebug($"Updating torrent info from debrid provider");
                    
                    var updateTime = Settings.Get.Provider.CheckInterval * 3;

                    if (updateTime < 30)
                    {
                        updateTime = 30;
                    }

                    if (RdtHub.HasConnections)
                    {
                        updateTime = Settings.Get.Provider.CheckInterval;

                        if (updateTime < 5)
                        {
                            updateTime = 5;
                        }
                    }

                    _nextUpdate = DateTime.UtcNow.AddSeconds(updateTime);

                    await torrentService.UpdateRdData();

                    logger.LogDebug($"Finished updating torrent info from debrid provider, next update in {updateTime} seconds");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unexpected error occurred in ProviderUpdater: {ex.Message}");
            }

            var delaySeconds = Settings.Get.Provider.Provider switch
            {
                Provider.TorBox => 10,
                _ => 1
            };

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }

        logger.LogInformation("ProviderUpdater stopped.");
    }
}