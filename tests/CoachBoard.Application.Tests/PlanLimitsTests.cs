using CoachBoard.Application.Services;
using CoachBoard.Domain.Enums;
using FluentAssertions;

namespace CoachBoard.Application.Tests;

public class PlanLimitsTests
{
    [Fact]
    public void FreePlan_ShouldReturnCorrectLimits()
    {
        // Arrange
        var provider = new PlanLimitsProvider();

        // Act
        var limits = provider.GetLimits(SubscriptionPlan.Free);

        // Assert
        limits.MaxAthletes.Should().Be(5);
        limits.MaxRoutines.Should().Be(20);
        limits.MaxCoaches.Should().Be(1);
    }

    [Fact]
    public void ProPlan_ShouldReturnUnlimited()
    {
        // Arrange
        var provider = new PlanLimitsProvider();
        const int UNLIMITED = -1;

        // Act
        var limits = provider.GetLimits(SubscriptionPlan.Pro);

        // Assert
        limits.MaxAthletes.Should().Be(UNLIMITED);
        limits.MaxRoutines.Should().Be(UNLIMITED);
        limits.MaxCoaches.Should().Be(UNLIMITED);
    }

    [Fact]
    public void UnknownPlan_ShouldDefaultToFreeLimits()
    {
        // Arrange
        var provider = new PlanLimitsProvider();
        var unknownPlan = (SubscriptionPlan)999;

        // Act
        var limits = provider.GetLimits(unknownPlan);

        // Assert
        limits.MaxAthletes.Should().Be(5);
        limits.MaxRoutines.Should().Be(20);
        limits.MaxCoaches.Should().Be(1);
    }
}
