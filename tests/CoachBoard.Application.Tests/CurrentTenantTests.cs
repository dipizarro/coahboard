using System.Security.Claims;
using CoachBoard.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CoachBoard.Application.Tests;

public class CurrentTenantTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentTenant _currentTenant;

    public CurrentTenantTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _currentTenant = new CurrentTenant(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void TenantId_WhenTidClaimExists_ReturnsParsedId()
    {
        // Arrange
        var claims = new[] { new Claim("tid", "123") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentTenant.TenantId;

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void TenantId_WhenTenantIdClaimExists_ReturnsParsedId()
    {
        // Arrange
        var claims = new[] { new Claim("tenantId", "456") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentTenant.TenantId;

        // Assert
        result.Should().Be(456);
    }

    [Fact]
    public void TenantId_WhenNoClaimExists_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentTenant.TenantId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TenantId_WhenInvalidClaimValue_ReturnsNull()
    {
        // Arrange
        var claims = new[] { new Claim("tid", "abc") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentTenant.TenantId;

        // Assert
        result.Should().BeNull();
    }
}
