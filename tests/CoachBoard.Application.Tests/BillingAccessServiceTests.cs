using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Services;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoachBoard.Application.Tests;

public class BillingAccessServiceTests
{
    private readonly Mock<IRepository<Subscription>> _mockRepo;
    private readonly Mock<ICurrentTenant> _mockTenant;
    private readonly BillingAccessService _service;

    public BillingAccessServiceTests()
    {
        _mockRepo = new Mock<IRepository<Subscription>>();
        _mockTenant = new Mock<ICurrentTenant>();
        _service = new BillingAccessService(_mockRepo.Object, _mockTenant.Object);
    }

    private void SetupTenant(int tenantId)
    {
        _mockTenant.Setup(t => t.TenantId).Returns(tenantId);
    }

    private void SetupSubscriptions(List<Subscription> subs)
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(subs);
    }

    [Fact]
    public async Task CanAccessPro_ActiveSubscription_ReturnsTrue()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>
        {
            new Subscription { Status = SubscriptionStatus.Active }
        });

        var result = await _service.CanAccessProAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessPro_TrialingSubscription_ReturnsTrue()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>
        {
            new Subscription { Status = SubscriptionStatus.Trialing }
        });

        var result = await _service.CanAccessProAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessPro_PastDue_InGracePeriod_ReturnsTrue()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>
        {
            new Subscription 
            { 
                Status = SubscriptionStatus.PastDue,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(1) // Future
            }
        });

        var result = await _service.CanAccessProAsync();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessPro_PastDue_ExpiredGracePeriod_ReturnsFalse()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>
        {
            new Subscription 
            { 
                Status = SubscriptionStatus.PastDue,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(-1) // Past
            }
        });

        var result = await _service.CanAccessProAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessPro_Canceled_ReturnsFalse()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>
        {
            new Subscription { Status = SubscriptionStatus.Canceled }
        });

        var result = await _service.CanAccessProAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessPro_NoSubscription_ReturnsFalse()
    {
        SetupTenant(1);
        SetupSubscriptions(new List<Subscription>());

        var result = await _service.CanAccessProAsync();
        result.Should().BeFalse();
    }
}
