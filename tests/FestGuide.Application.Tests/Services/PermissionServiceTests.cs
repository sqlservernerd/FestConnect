using FluentAssertions;
using Moq;
using FestGuide.Application.Authorization;
using FestGuide.Application.Dtos;
using FestGuide.Application.Services;
using FestGuide.DataAccess.Abstractions;
using FestGuide.Domain.Entities;
using FestGuide.Domain.Enums;
using FestGuide.Domain.Exceptions;
using FestGuide.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FestGuide.Application.Tests.Services;

public class PermissionServiceTests
{
    private readonly Mock<IFestivalPermissionRepository> _mockPermissionRepo;
    private readonly Mock<IFestivalRepository> _mockFestivalRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IFestivalAuthorizationService> _mockAuthService;
    private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<ILogger<PermissionService>> _mockLogger;
    private readonly PermissionService _sut;
    private readonly DateTime _now = new(2026, 1, 20, 12, 0, 0, DateTimeKind.Utc);

    public PermissionServiceTests()
    {
        _mockPermissionRepo = new Mock<IFestivalPermissionRepository>();
        _mockFestivalRepo = new Mock<IFestivalRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockAuthService = new Mock<IFestivalAuthorizationService>();
        _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        _mockLogger = new Mock<ILogger<PermissionService>>();

        _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(_now);

        _sut = new PermissionService(
            _mockPermissionRepo.Object,
            _mockFestivalRepo.Object,
            _mockUserRepo.Object,
            _mockAuthService.Object,
            _mockDateTimeProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetByFestivalAsync_WithValidFestivalId_ReturnsPermissions()
    {
        // Arrange
        var festivalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var permissions = new List<FestivalPermission>
        {
            new()
            {
                FestivalPermissionId = Guid.NewGuid(),
                FestivalId = festivalId,
                UserId = user1Id,
                Role = FestivalRole.Manager,
                Scope = PermissionScope.Artists,
                IsPending = false,
                IsRevoked = false
            },
            new()
            {
                FestivalPermissionId = Guid.NewGuid(),
                FestivalId = festivalId,
                UserId = user2Id,
                Role = FestivalRole.Viewer,
                Scope = PermissionScope.All,
                IsPending = true,
                IsRevoked = false
            }
        };

        var users = new List<User>
        {
            new() { UserId = user1Id, Email = "user1@test.com", DisplayName = "User 1" },
            new() { UserId = user2Id, Email = "user2@test.com", DisplayName = "User 2" }
        };

        _mockAuthService.Setup(x => x.CanViewFestivalAsync(userId, festivalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockPermissionRepo.Setup(x => x.GetActiveByFestivalAsync(festivalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        _mockUserRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetByFestivalAsync(festivalId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].UserEmail.Should().Be("user1@test.com");
        result[0].UserDisplayName.Should().Be("User 1");
        result[1].UserEmail.Should().Be("user2@test.com");
        result[1].UserDisplayName.Should().Be("User 2");
        
        // Verify batch fetch was used (called once, not per permission)
        _mockUserRepo.Verify(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByFestivalAsync_WithoutPermission_ThrowsForbiddenException()
    {
        // Arrange
        var festivalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockAuthService.Setup(x => x.CanViewFestivalAsync(userId, festivalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _sut.GetByFestivalAsync(festivalId, userId);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You do not have permission to view this festival's permissions.");
    }

    [Fact]
    public async Task GetPendingInvitationsAsync_WithPendingInvitations_ReturnsInvitations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var festivalId1 = Guid.NewGuid();
        var festivalId2 = Guid.NewGuid();
        var inviterId = Guid.NewGuid();

        var permissions = new List<FestivalPermission>
        {
            new()
            {
                FestivalPermissionId = Guid.NewGuid(),
                FestivalId = festivalId1,
                UserId = userId,
                Role = FestivalRole.Manager,
                Scope = PermissionScope.Artists,
                IsPending = true,
                IsRevoked = false,
                InvitedByUserId = inviterId,
                CreatedAtUtc = _now.AddDays(-1)
            },
            new()
            {
                FestivalPermissionId = Guid.NewGuid(),
                FestivalId = festivalId2,
                UserId = userId,
                Role = FestivalRole.Viewer,
                Scope = PermissionScope.All,
                IsPending = true,
                IsRevoked = false,
                InvitedByUserId = inviterId,
                CreatedAtUtc = _now.AddDays(-2)
            },
            new()
            {
                FestivalPermissionId = Guid.NewGuid(),
                FestivalId = festivalId1,
                UserId = userId,
                Role = FestivalRole.Viewer,
                Scope = PermissionScope.All,
                IsPending = false, // Not pending
                IsRevoked = false,
                CreatedAtUtc = _now.AddDays(-3)
            }
        };

        var festivals = new List<Festival>
        {
            new() { FestivalId = festivalId1, Name = "Festival 1", OwnerUserId = Guid.NewGuid() },
            new() { FestivalId = festivalId2, Name = "Festival 2", OwnerUserId = Guid.NewGuid() }
        };

        var inviter = new User { UserId = inviterId, Email = "inviter@test.com", DisplayName = "Inviter" };

        _mockPermissionRepo.Setup(x => x.GetByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        _mockFestivalRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(festivals);
        _mockUserRepo.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { inviter });

        // Act
        var result = await _sut.GetPendingInvitationsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only pending invitations
        result[0].FestivalName.Should().Be("Festival 1");
        result[0].InvitedByUserName.Should().Be("Inviter");
        result[1].FestivalName.Should().Be("Festival 2");
        
        // Verify batch fetches were used
        _mockFestivalRepo.Verify(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepo.Verify(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
