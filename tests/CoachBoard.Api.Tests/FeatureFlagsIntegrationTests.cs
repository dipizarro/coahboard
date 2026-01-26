using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace CoachBoard.Api.Tests;

public class FeatureFlagsIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public FeatureFlagsIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldRespectTenantIsolation()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        
        // Tenants 10 and 11 are seeded by factory.
        var tenantAId = 10;
        var tenantBId = 11;

        var flagA = new FeatureFlag { Name = "BetaFeature", IsEnabled = true, TenantId = tenantAId };
        var flagB = new FeatureFlag { Name = "BetaFeature", IsEnabled = false, TenantId = tenantBId };
        
        // Use DbContext directly to verify data insertion, simpler than Repository for setup
        context.FeatureFlags.AddRange(flagA, flagB);
        await context.SaveChangesAsync();

        // Check Tenant A (Expect True)
        await CheckFlagForTenant(tenantAId, "BetaFeature", expected: true);
        
        // Check Tenant B (Expect False)
        await CheckFlagForTenant(tenantBId, "BetaFeature", expected: false);
        
        // Check Missing Flag (Expect False)
        await CheckFlagForTenant(tenantAId, "MissingFeature", expected: false);
    }

    private async Task CheckFlagForTenant(int tenantId, string featureName, bool expected)
    {
        using var scope = _factory.Services.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var featureFlags = scope.ServiceProvider.GetRequiredService<IFeatureFlags>();

        // Set Tenant Context via HttpContextAccessor
        var claims = new[] { new Claim("tid", tenantId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        httpContextAccessor.HttpContext = new DefaultHttpContext { User = principal };

        // Act
        var result = await featureFlags.IsEnabledAsync(featureName);

        // Assert
        result.Should().Be(expected, $"Tenant {tenantId} should see '{featureName}' as {expected}");
    }
}
