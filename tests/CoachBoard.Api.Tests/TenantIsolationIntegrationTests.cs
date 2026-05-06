using System.Net;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class TenantIsolationIntegrationTests : BaseIntegrationTest
{
    public TenantIsolationIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClient_OwnedByOtherTenant_ReturnsNotFound()
    {
        // Arrange
        // Seed a client for Tenant B (User B)
        var clientB = new Client 
        { 
            FullName = "Client B", 
            Email = "clientB@test.local", 
            Phone = "123", 
            CoachId = 11, 
            TenantId = 11 
        };
        Db.Clients.Add(clientB);
        await Db.SaveChangesAsync();

        // Act
        // Login as User A (Tenant A)
        await AuthenticateAsUserAAsync();
        
        // Try to access Client B
        var response = await Client.GetAsync($"/api/clients/{clientB.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClients_OnlyReturnsTenantsOwnClients()
    {
        // Arrange
        var clientA = new Client { FullName = "Client A", Email = "a@test.local", Phone = "1", CoachId = 10, TenantId = 10 };
        var clientB = new Client { FullName = "Client B", Email = "b@test.local", Phone = "2", CoachId = 11, TenantId = 11 };
        
        Db.Clients.AddRange(clientA, clientB);
        await Db.SaveChangesAsync();

        // Act
        await AuthenticateAsUserAAsync();
        var response = await Client.GetAsync("/api/clients?coachId=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<dynamic>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }
    [Fact]
    public async Task Routine_TenantIsolation_FullScenario()
    {
        // 1. Authenticate as User A
        await AuthenticateAsUserAAsync();

        // 2. Create a Routine for Client 10 (Tenant A) using Exercise 10 (Seeded in Factory)
        var createDto = new RoutineCreateDto(
            "Routine A", 
            10, 
            new List<RoutineItemDto> { new RoutineItemDto(10, 3, 10, 1, "Notes") }
        );
        var createResponse = await Client.PostAsJsonAsync("/api/routines", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var routine = await createResponse.Content.ReadFromJsonAsync<RoutineReadDto>();
        routine.Should().NotBeNull();

        // 3. Authenticate as User B
        await AuthenticateAsUserBAsync();

        // 4. Attempt to GET the Routine by ID -> Expect 404 Not Found (Isolation at Repo level)
        var getByIdResponse = await Client.GetAsync($"/api/routines/{routine!.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 5. Attempt to GET all Routines for Client 10 -> Expect 403 Forbidden (Controller checks if Client belongs to Coach)
        var getByClientResponse = await Client.GetAsync("/api/routines?clientId=10");
        getByClientResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 6. Authenticate back as User A
        await AuthenticateAsUserAAsync();

        // 7. GET the Routine by ID -> Expect 200 OK
        var getByIdSuccessResponse = await Client.GetAsync($"/api/routines/{routine.Id}");
        getByIdSuccessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalRoutine = await getByIdSuccessResponse.Content.ReadFromJsonAsync<RoutineReadDto>();
        finalRoutine!.Title.Should().Be("Routine A");
    }

    [Fact]
    public async Task GetRoutines_WhenClientBelongsToAnotherCoachInSameTenant_ReturnsForbidden()
    {
        var (clientId, _) = await CreateRoutineForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var response = await Client.GetAsync($"/api/routines?clientId={clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoutineById_WhenRoutineBelongsToAnotherCoachInSameTenant_ReturnsForbidden()
    {
        var (_, routineId) = await CreateRoutineForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var response = await Client.GetAsync($"/api/routines/{routineId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateRoutine_WhenClientBelongsToAnotherCoachInSameTenant_ReturnsForbidden()
    {
        var (clientId, _) = await CreateRoutineForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var dto = new RoutineCreateDto(
            "Cross Coach Create",
            clientId,
            new List<RoutineItemDto> { new RoutineItemDto(10, 3, 10, 1, "Notes") });

        var response = await Client.PostAsJsonAsync("/api/routines", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateRoutine_WhenRoutineBelongsToAnotherCoachInSameTenant_ReturnsForbidden()
    {
        var (_, routineId) = await CreateRoutineForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var dto = new RoutineUpdateDto(
            "Cross Coach Update",
            new List<RoutineItemDto> { new RoutineItemDto(10, 4, 8, 1, "Updated") });

        var response = await Client.PutAsJsonAsync($"/api/routines/{routineId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteRoutine_WhenRoutineBelongsToAnotherCoachInSameTenant_ReturnsForbidden()
    {
        var (_, routineId) = await CreateRoutineForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var response = await Client.DeleteAsync($"/api/routines/{routineId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<(int ClientId, int RoutineId)> CreateRoutineForAnotherCoachInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");

        var coach = new Coach
        {
            Name = $"Other Coach {suffix}",
            Specialty = "General",
            TenantId = 10
        };
        Db.Coaches.Add(coach);
        await Db.SaveChangesAsync();

        var client = new Client
        {
            FullName = $"Other Coach Client {suffix}",
            Email = $"other-client-{suffix}@test.local",
            CoachId = coach.Id,
            TenantId = 10
        };
        Db.Clients.Add(client);
        await Db.SaveChangesAsync();

        var routine = new Routine
        {
            Title = $"Other Coach Routine {suffix}",
            ClientId = client.Id,
            TenantId = 10
        };
        Db.Routines.Add(routine);
        await Db.SaveChangesAsync();

        return (client.Id, routine.Id);
    }
}


