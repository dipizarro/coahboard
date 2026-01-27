using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CoachBoard.Domain.Tests;

public class SubscriptionTests
{
    [Theory]
    [InlineData(SubscriptionStatus.Active, true)]
    [InlineData(SubscriptionStatus.Trialing, true)]
    [InlineData(SubscriptionStatus.PastDue, false)]
    [InlineData(SubscriptionStatus.Canceled, false)]
    [InlineData(SubscriptionStatus.Unpaid, false)]
    [InlineData(SubscriptionStatus.None, false)]
    public void IsActive_ReturnsExpectedResult(SubscriptionStatus status, bool expected)
    {
        // Arrange
        var subscription = new Subscription { Status = status };

        // Act
        var result = subscription.IsActive();

        // Assert
        result.Should().Be(expected);
    }
}
