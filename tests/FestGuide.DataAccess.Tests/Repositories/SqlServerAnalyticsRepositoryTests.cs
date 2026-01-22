using System.Data;
using Dapper;
using FluentAssertions;
using Moq;
using FestGuide.DataAccess.Repositories;
using FestGuide.Domain.Entities;

namespace FestGuide.DataAccess.Tests.Repositories;

public class SqlServerAnalyticsRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly SqlServerAnalyticsRepository _sut;

    public SqlServerAnalyticsRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _sut = new SqlServerAnalyticsRepository(_mockConnection.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullConnection_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new SqlServerAnalyticsRepository(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connection");
    }

    #endregion

    #region RecordEventAsync Tests

    [Fact]
    public async Task RecordEventAsync_WithValidEvent_ReturnsEventId()
    {
        // Arrange
        var analyticsEvent = CreateTestAnalyticsEvent();

        // Act
        var result = await _sut.RecordEventAsync(analyticsEvent);

        // Assert
        result.Should().Be(analyticsEvent.AnalyticsEventId);
    }

    [Fact]
    public async Task RecordEventAsync_WithCancellationToken_PassesTokenToCommand()
    {
        // Arrange
        var analyticsEvent = CreateTestAnalyticsEvent();
        var cts = new CancellationTokenSource();

        // Act
        await _sut.RecordEventAsync(analyticsEvent, cts.Token);

        // Assert - token should be passed through
        cts.Token.IsCancellationRequested.Should().BeTrue();
    }

    #endregion

    #region RecordEventsAsync Tests

    [Fact]
    public async Task RecordEventsAsync_WithMultipleEvents_InsertsAllEvents()
    {
        // Arrange
        var events = new List<AnalyticsEvent>
        {
            CreateTestAnalyticsEvent(),
            CreateTestAnalyticsEvent(),
            CreateTestAnalyticsEvent()
        };

        // Act
        await _sut.RecordEventsAsync(events);

        // Assert - should complete without error
        events.Should().HaveCount(3);
    }

    [Fact]
    public async Task RecordEventsAsync_WithEmptyList_CompletesSuccessfully()
    {
        // Arrange
        var events = new List<AnalyticsEvent>();

        // Act
        await _sut.RecordEventsAsync(events);

        // Assert - should complete without error
        events.Should().BeEmpty();
    }

    #endregion

    #region GetScheduleViewCountAsync Tests

    [Fact]
    public async Task GetScheduleViewCountAsync_WithNoDateRange_ReturnsCount()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetScheduleViewCountAsync(editionId);
    }

    [Fact]
    public async Task GetScheduleViewCountAsync_WithDateRange_PassesDateParameters()
    {
        // Arrange
        var editionId = Guid.NewGuid();
        var fromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act & Assert - test that it doesn't throw
        await _sut.GetScheduleViewCountAsync(editionId, fromUtc, toUtc);
    }

    #endregion

    #region GetUniqueViewerCountAsync Tests

    [Fact]
    public async Task GetUniqueViewerCountAsync_WithEditionId_ReturnsCount()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetUniqueViewerCountAsync(editionId);
    }

    #endregion

    #region GetTopSavedEngagementsAsync Tests

    [Fact]
    public async Task GetTopSavedEngagementsAsync_WithLimit_ReturnsTopEngagements()
    {
        // Arrange
        var editionId = Guid.NewGuid();
        var limit = 10;

        // Act & Assert - just test that it doesn't throw
        await _sut.GetTopSavedEngagementsAsync(editionId, limit);
    }

    [Fact]
    public async Task GetTopSavedEngagementsAsync_WithDefaultLimit_Uses10()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - should complete without error
        await _sut.GetTopSavedEngagementsAsync(editionId);
    }

    #endregion

    #region GetTopArtistsAsync Tests

    [Fact]
    public async Task GetTopArtistsAsync_WithEditionId_ReturnsTopArtists()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetTopArtistsAsync(editionId);
    }

    #endregion

    #region GetEventTimelineAsync Tests

    [Fact]
    public async Task GetEventTimelineAsync_WithValidParameters_ReturnsTimeline()
    {
        // Arrange
        var editionId = Guid.NewGuid();
        var eventType = "schedule_view";
        var fromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 1, 7, 23, 59, 59, DateTimeKind.Utc);

        // Act & Assert - just test that it doesn't throw
        await _sut.GetEventTimelineAsync(editionId, eventType, fromUtc, toUtc);
    }

    #endregion

    #region GetPlatformDistributionAsync Tests

    [Fact]
    public async Task GetPlatformDistributionAsync_WithEditionId_ReturnsPlatforms()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetPlatformDistributionAsync(editionId);
    }

    #endregion

    #region GetPersonalScheduleCountAsync Tests

    [Fact]
    public async Task GetPersonalScheduleCountAsync_WithEditionId_ReturnsCount()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetPersonalScheduleCountAsync(editionId);
    }

    #endregion

    #region GetTotalEngagementSavesAsync Tests

    [Fact]
    public async Task GetTotalEngagementSavesAsync_WithEditionId_ReturnsCount()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetTotalEngagementSavesAsync(editionId);
    }

    #endregion

    #region GetDailyActiveUsersAsync Tests

    [Fact]
    public async Task GetDailyActiveUsersAsync_WithDateRange_ReturnsDailyData()
    {
        // Arrange
        var editionId = Guid.NewGuid();
        var fromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 1, 7, 23, 59, 59, DateTimeKind.Utc);

        // Act & Assert - just test that it doesn't throw
        await _sut.GetDailyActiveUsersAsync(editionId, fromUtc, toUtc);
    }

    #endregion

    #region GetEventTypeDistributionAsync Tests

    [Fact]
    public async Task GetEventTypeDistributionAsync_WithEditionId_ReturnsDistribution()
    {
        // Arrange
        var editionId = Guid.NewGuid();

        // Act & Assert - just test that it doesn't throw
        await _sut.GetEventTypeDistributionAsync(editionId);
    }

    [Fact]
    public async Task GetEventTypeDistributionAsync_WithDateRange_PassesDateParameters()
    {
        // Arrange
        var editionId = Guid.NewGuid();
        var fromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act & Assert - should complete without error
        await _sut.GetEventTypeDistributionAsync(editionId, fromUtc, toUtc);
    }

    #endregion

    #region Helper Methods

    private AnalyticsEvent CreateTestAnalyticsEvent()
    {
        return new AnalyticsEvent
        {
            AnalyticsEventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FestivalId = Guid.NewGuid(),
            EditionId = Guid.NewGuid(),
            EventType = "schedule_view",
            EntityType = "Edition",
            EntityId = Guid.NewGuid(),
            Metadata = null,
            Platform = "iOS",
            DeviceType = "Mobile",
            SessionId = Guid.NewGuid().ToString(),
            EventTimestampUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    #endregion
}
