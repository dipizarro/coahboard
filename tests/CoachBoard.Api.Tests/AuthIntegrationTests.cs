using System.Net;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClients_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/clients?coachId=10");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetClients_WithValidJwt_ReturnsOk()
    {
        await AuthenticateAsUserAAsync();
        
        var response = await Client.GetAsync("/api/clients?coachId=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
