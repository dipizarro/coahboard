using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Services;
using CoachBoard.Domain.Entities;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace CoachBoard.Application.Tests;

public class FeatureFlagsTests
{
    private readonly Mock<IRepository<FeatureFlag>> _repoMock;
    private readonly FeatureFlagsService _service;

    public FeatureFlagsTests()
    {
        _repoMock = new Mock<IRepository<FeatureFlag>>();
        _service = new FeatureFlagsService(_repoMock.Object);
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFlagExistsAndEnabled_ReturnsTrue()
    {
        // Arrange
        var flag = new FeatureFlag { Name = "NewFeature", IsEnabled = true };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FeatureFlag, bool>>>()))
            .ReturnsAsync(new[] { flag });

        // Act
        var result = await _service.IsEnabledAsync("NewFeature");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFlagExistsAndDisabled_ReturnsFalse()
    {
        // Arrange
        var flag = new FeatureFlag { Name = "NewFeature", IsEnabled = false };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FeatureFlag, bool>>>()))
            .ReturnsAsync(new[] { flag });

        // Act
        var result = await _service.IsEnabledAsync("NewFeature");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFlagDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FeatureFlag, bool>>>()))
            .ReturnsAsync(new List<FeatureFlag>());

        // Act
        var result = await _service.IsEnabledAsync("NonExistentFeature");

        // Assert
        result.Should().BeFalse();
    }
}
