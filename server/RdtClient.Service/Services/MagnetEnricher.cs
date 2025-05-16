using System.Web;
using Microsoft.Extensions.Logging;

namespace RdtClient.Service.Services;

public interface IMagnetEnricher
{
    Task<String> EnrichMagnetLink(String magnetLink);
}

/// <summary>
/// Enriches magnet links by adding trackers from the tracker list grabber. 
/// </summary>
public class MagnetEnricher(ILogger<MagnetEnricher> logger, ITrackerListGrabber trackerListGrabber) : IMagnetEnricher
{
    /// <summary>
    /// Add trackers from the tracker list grabber to the magnet link.
    /// </summary>
    /// <param name="magnetLink">Magnet link to add trackres to. Is not modified</param>
    /// <returns>Magnet link with additional trackers</returns>
    public async Task<String> EnrichMagnetLink(String magnetLink)
    {
        String[] newTrackers;

        try
        {
            newTrackers = await trackerListGrabber.GetTrackers();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}, trying to enrich {Magnet}", ex.Message, magnetLink);

            return magnetLink;
        }

        var uri = new Uri(magnetLink);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var existingTrackers = query.GetValues("tr") ?? [];
        var allTrackers = existingTrackers.Concat(newTrackers).Distinct(StringComparer.OrdinalIgnoreCase);

        var trackerQuery = String.Join("&tr=", allTrackers.Select(Uri.EscapeDataString));

        if (!String.IsNullOrEmpty(trackerQuery))
        {
            trackerQuery = "&tr=" + trackerQuery;
        }

        var baseWithoutTrackers = magnetLink.Split("&tr=")[0];

        var separator = baseWithoutTrackers.Contains('?') ? "&" : "?";

        return baseWithoutTrackers + separator + trackerQuery.TrimStart('&');
    }
}
