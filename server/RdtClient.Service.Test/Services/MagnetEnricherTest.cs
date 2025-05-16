using Microsoft.Extensions.Logging;
using Moq;
using RdtClient.Service.Services;

namespace RdtClient.Service.Test.Services;

public class MagnetEnricherTest: IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ILogger<MagnetEnricher>> _loggerMock;
    private readonly Mock<ITrackerListGrabber> _trackerListGrabberMock;

    public MagnetEnricherTest()
    {
        _mockRepository = new(MockBehavior.Strict);
        _loggerMock = _mockRepository.Create<ILogger<MagnetEnricher>>(MockBehavior.Loose);
        _trackerListGrabberMock = _mockRepository.Create<ITrackerListGrabber>();
    }

    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    private const String TestMagnetLink = "magnet:?xt=urn:btih:1234567890123456789012345678901234567890&dn=TestFile&tr=http%3A%2F%2Ftracker1.com%2Fannounce&tr=http%3A%2F%2Ftracker2.com%2Fannounce";
    
    [Fact]
    public async Task EnrichMagnetLink_AddsNoTrackers_WhenNoTrackersFromTrackerGrabber()
    {
        // Arrange
        SetupTrackerListGrabber([]);
        
        var magnetEnricher = new MagnetEnricher(_loggerMock.Object, _trackerListGrabberMock.Object);
        
        // Act
        var enriched = await magnetEnricher.EnrichMagnetLink(TestMagnetLink);
        
        // Assert
        Assert.Equal(TestMagnetLink, enriched);
    }

    [Fact]
    public async Task EnrichMagnetLink_AddsTrackers_WhenTrackersFromTrackerGrabber()
    {
        // Arrange
        SetupTrackerListGrabber(["http://new-tracker.com/announce"]);
        
        var magnetEnricher = new MagnetEnricher(_loggerMock.Object, _trackerListGrabberMock.Object);
        
        // Act
        var enriched = await magnetEnricher.EnrichMagnetLink(TestMagnetLink);
        
        // Assert
        Assert.Equal(TestMagnetLink + $"&tr={Uri.EscapeDataString("http://new-tracker.com/announce")}", enriched);
    }
    
    [Fact]
    public async Task EnrichMagnetLink_DoesNotAddDuplicateTrackers_WhenTrackersFromTrackerGrabberAlreadyPresent()
    {
        // Arrange
        SetupTrackerListGrabber(["http://new-tracker.com/announce", "http://tracker1.com/announce"]);
        
        var magnetEnricher = new MagnetEnricher(_loggerMock.Object, _trackerListGrabberMock.Object);
        
        // Act
        var enriched = await magnetEnricher.EnrichMagnetLink(TestMagnetLink);
        
        // Assert
        Assert.Equal(TestMagnetLink + $"&tr={Uri.EscapeDataString("http://new-tracker.com/announce")}", enriched);
    }

    [Fact]
    public async Task EnrichMagnetLink_ReturnsOriginalLink_WhenTrackerGrabberThrows()
    {
        // Arrange
        _trackerListGrabberMock
            .Setup(t => t.GetTrackers())
            .ThrowsAsync(new Exception("Something went wrong!"));
        
        var magnetEnricher = new MagnetEnricher(_loggerMock.Object, _trackerListGrabberMock.Object);
        
        // Act
        var enriched = await magnetEnricher.EnrichMagnetLink(TestMagnetLink);
        
        // Assert
        Assert.Same(TestMagnetLink, enriched);
    }
    
    private void SetupTrackerListGrabber(String[] trackerList)
    {
        _trackerListGrabberMock
            .Setup(t => t.GetTrackers())
            .ReturnsAsync(trackerList)
            .Verifiable();
    }
}
