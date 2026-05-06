using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class ClientProgressIntegrationTests : BaseIntegrationTest
{
    public ClientProgressIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ProgressCrud_ForOwnClient_Succeeds()
    {
        await AuthenticateAsUserAAsync();

        var create = new ClientProgressCreateDto(
            RecordedAt: new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
            WeightKg: 82.5m,
            HeightCm: 178m,
            BodyFatPercentage: 18.4m,
            WaistCm: 88m,
            RestingHeartRate: 62,
            Notes: "Initial measurement");

        var createResponse = await Client.PostAsJsonAsync("/api/clients/10/progress", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ClientProgressReadDto>();
        created.Should().NotBeNull();
        created!.ClientId.Should().Be(10);
        created.WeightKg.Should().Be(82.5m);

        var listResponse = await Client.GetAsync("/api/clients/10/progress");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await listResponse.Content.ReadFromJsonAsync<List<ClientProgressReadDto>>();
        records.Should().ContainSingle(x => x.Id == created.Id);

        var update = new ClientProgressUpdateDto(
            RecordedAt: create.RecordedAt,
            WeightKg: 81.9m,
            HeightCm: 178m,
            BodyFatPercentage: 17.8m,
            WaistCm: 86.5m,
            RestingHeartRate: 60,
            Notes: "Updated measurement");

        var updateResponse = await Client.PutAsJsonAsync($"/api/clients/10/progress/{created.Id}", update);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ClientProgressReadDto>();
        updated!.WeightKg.Should().Be(81.9m);
        updated.Notes.Should().Be("Updated measurement");

        var deleteResponse = await Client.DeleteAsync($"/api/clients/10/progress/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeletedResponse = await Client.GetAsync($"/api/clients/10/progress/{created.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProgressEndpoints_WhenClientBelongsToAnotherCoachInSameTenant_ReturnForbidden()
    {
        var (clientId, progressId) = await CreateProgressForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var create = new ClientProgressCreateDto(
            RecordedAt: DateTime.UtcNow,
            WeightKg: 75m);

        (await Client.GetAsync($"/api/clients/{clientId}/progress")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.GetAsync($"/api/clients/{clientId}/progress/{progressId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PostAsJsonAsync($"/api/clients/{clientId}/progress", create)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PutAsJsonAsync($"/api/clients/{clientId}/progress/{progressId}", new ClientProgressUpdateDto(DateTime.UtcNow, WeightKg: 74m))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.DeleteAsync($"/api/clients/{clientId}/progress/{progressId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProgressEndpoints_WhenAdminAccessesAnotherCoachInSameTenant_Succeeds()
    {
        var (clientId, progressId) = await CreateProgressForAnotherCoachInTenantAAsync();
        await AuthenticateAsAdminInTenantAAsync();

        var getResponse = await Client.GetAsync($"/api/clients/{clientId}/progress/{progressId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var record = await getResponse.Content.ReadFromJsonAsync<ClientProgressReadDto>();
        record!.Id.Should().Be(progressId);
    }

    private async Task<(int ClientId, int ProgressId)> CreateProgressForAnotherCoachInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");

        var coach = new Coach
        {
            Name = $"Progress Coach {suffix}",
            Specialty = "General",
            TenantId = 10
        };
        Db.Coaches.Add(coach);
        await Db.SaveChangesAsync();

        var client = new Client
        {
            FullName = $"Progress Client {suffix}",
            Email = $"progress-client-{suffix}@test.local",
            CoachId = coach.Id,
            TenantId = 10
        };
        Db.Clients.Add(client);
        await Db.SaveChangesAsync();

        var record = new ClientProgressRecord
        {
            ClientId = client.Id,
            RecordedAt = DateTime.UtcNow,
            WeightKg = 80m,
            Notes = "Other coach record"
        };
        Db.ClientProgressRecords.Add(record);
        await Db.SaveChangesAsync();

        return (client.Id, record.Id);
    }

    private async Task AuthenticateAsAdminInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"admin-{suffix}@test.local";
        var password = "P@ssw0rd!";

        Db.Users.Add(new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 11),
            Role = "Admin",
            TenantId = 10
        });
        await Db.SaveChangesAsync();

        var token = await Factory.LoginAsAsync(Client, email, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
