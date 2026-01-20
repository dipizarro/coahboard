using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class AuthIntegrationTests
{
    [Fact]
    public async Task GetClients_WithoutAuthorization_ReturnsUnauthorized()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/clients?coachId=1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetClients_WithValidJwt_ReturnsOk()
    {
        await using var factory = new CustomWebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var loginRequest = new LoginRequest("coach@test.local", "P@ssw0rd!");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
        var response = await client.GetAsync("/api/clients?coachId=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
