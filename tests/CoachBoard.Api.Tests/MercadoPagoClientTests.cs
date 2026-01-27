using CoachBoard.Application.Interfaces;
using CoachBoard.Infrastructure.Payment;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoachBoard.Api.Tests;

public class MercadoPagoClientTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public MercadoPagoClientTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DI_CanResolveIMercadoPagoClient()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var client = scope.ServiceProvider.GetService<IMercadoPagoClient>();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<MercadoPagoClient>();
    }

    [Fact]
    public void DI_CanResolveOptions()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var options = scope.ServiceProvider.GetService<IOptions<MercadoPagoOptions>>();

        // Assert
        options.Should().NotBeNull();
        options!.Value.Should().NotBeNull();
        // Since we are using CustomWebApplicationFactory without explicit overrides for MP, 
        // values will be default or empty string, but the object should exist.
        options.Value.Currency.Should().Be("CLP"); 
    }
}
