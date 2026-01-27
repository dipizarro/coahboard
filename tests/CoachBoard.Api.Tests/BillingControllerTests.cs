using CoachBoard.Api.Controllers;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CoachBoard.Api.Tests;

public class BillingControllerTests
{
    private readonly Mock<IMercadoPagoClient> _mpClientMock;
    private readonly Mock<ICurrentTenant> _currentTenantMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IRepository<Subscription>> _subRepoMock;
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly BillingController _controller;

    public BillingControllerTests()
    {
        _mpClientMock = new Mock<IMercadoPagoClient>();
        _currentTenantMock = new Mock<ICurrentTenant>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _subRepoMock = new Mock<IRepository<Subscription>>();
        _tenantRepoMock = new Mock<ITenantRepository>();

        _controller = new BillingController(
            _mpClientMock.Object, 
            _currentTenantMock.Object, 
            _currentUserMock.Object, 
            _subRepoMock.Object, 
            _tenantRepoMock.Object);
    }

    [Fact]
    public async Task CreateProCheckout_WhenTenantMissing_ReturnsBadRequest()
    {
        // Arrange
        _currentTenantMock.Setup(x => x.TenantId).Returns((int?)null);

        // Act
        var result = await _controller.CreateProCheckout();

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateProCheckout_WhenSuccessful_ReturnsResponseAndCreatesPendingSubscription()
    {
        // Arrange
        int tenantId = 10;
        int userId = 5;
        string expectedUrl = "https://mp.com/checkout";

        _currentTenantMock.Setup(x => x.TenantId).Returns(tenantId);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        
        _mpClientMock.Setup(x => x.CreateProCheckoutAsync(userId, It.IsAny<string>(), tenantId))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.CreateProCheckout();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CheckoutResponse>().Subject;

        response.InitPoint.Should().Be(expectedUrl);
        response.ProviderReferenceId.Should().NotBeNullOrEmpty();

        _subRepoMock.Verify(x => x.AddAsync(It.Is<Subscription>(s => 
            s.TenantId == tenantId &&
            s.Status == SubscriptionStatus.Pending &&
            s.Provider == "MercadoPago"
        )), Times.Once);

        _subRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
