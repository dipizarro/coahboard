using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using FluentAssertions;

namespace CoachBoard.Application.Tests;

public class TenantTests
{
    [Fact]
    public void NewTenant_ShouldDefaultToFreePlan()
    {
        // Arrange & Act
        var tenant = new Tenant { Name = "Test Tenant" };

        // Assert
        tenant.Plan.Should().Be(SubscriptionPlan.Free);
    }

    [Fact]
    public void Tenant_CanBeAssignedProPlan()
    {
        // Arrange
        var tenant = new Tenant 
        { 
            Name = "Pro Tenant",
            Plan = SubscriptionPlan.Pro
        };

        // Assert
        tenant.Plan.Should().Be(SubscriptionPlan.Pro);
    }

    [Fact]
    public void Tenant_PlanCanBeChanged()
    {
        // Arrange
        var tenant = new Tenant { Name = "Test Tenant" };
        tenant.Plan.Should().Be(SubscriptionPlan.Free);

        // Act
        tenant.Plan = SubscriptionPlan.Pro;

        // Assert
        tenant.Plan.Should().Be(SubscriptionPlan.Pro);
    }
}
