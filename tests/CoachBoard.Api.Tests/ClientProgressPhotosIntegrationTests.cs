using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class ClientProgressPhotosIntegrationTests : BaseIntegrationTest
{
    public ClientProgressPhotosIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task PhotoUploadListAndDelete_ForOwnClient_Succeeds()
    {
        await AuthenticateAsUserAAsync();
        var progress = await CreateProgressRecordAsync(clientId: 10);

        var upload = CreatePhotoMultipart("progress.jpg");
        upload.Add(new StringContent(progress.Id.ToString()), "clientProgressRecordId");
        upload.Add(new StringContent("Front"), "photoType");
        upload.Add(new StringContent(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc).ToString("O")), "takenAt");
        upload.Add(new StringContent("Initial front photo"), "notes");

        var createResponse = await Client.PostAsync("/api/clients/10/photos", upload);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ClientProgressPhotoReadDto>();
        created.Should().NotBeNull();
        created!.ClientId.Should().Be(10);
        created.ClientProgressRecordId.Should().Be(progress.Id);
        created.PhotoUrl.Should().Contain("/uploads/progress/");
        created.PhotoType.Should().Be("Front");

        var listResponse = await Client.GetAsync("/api/clients/10/photos");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var photos = await listResponse.Content.ReadFromJsonAsync<List<ClientProgressPhotoReadDto>>();
        photos.Should().ContainSingle(x => x.Id == created.Id);

        var deleteResponse = await Client.DeleteAsync($"/api/clients/10/photos/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listAfterDeleteResponse = await Client.GetAsync("/api/clients/10/photos");
        var photosAfterDelete = await listAfterDeleteResponse.Content.ReadFromJsonAsync<List<ClientProgressPhotoReadDto>>();
        photosAfterDelete.Should().NotContain(x => x.Id == created.Id);
    }

    [Fact]
    public async Task UploadPhoto_WithInvalidExtension_ReturnsBadRequest()
    {
        await AuthenticateAsUserAAsync();
        var upload = CreatePhotoMultipart("progress.gif");

        var response = await Client.PostAsync("/api/clients/10/photos", upload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PhotoEndpoints_WhenClientBelongsToAnotherCoachInSameTenant_ReturnForbidden()
    {
        var (clientId, photoId) = await CreatePhotoForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        (await Client.GetAsync($"/api/clients/{clientId}/photos")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.PostAsync($"/api/clients/{clientId}/photos", CreatePhotoMultipart("blocked.jpg"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.DeleteAsync($"/api/clients/{clientId}/photos/{photoId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<ClientProgressRecord> CreateProgressRecordAsync(int clientId)
    {
        var record = new ClientProgressRecord
        {
            ClientId = clientId,
            RecordedAt = DateTime.UtcNow,
            WeightKg = 80m,
            Notes = "Photo test measurement"
        };
        Db.ClientProgressRecords.Add(record);
        await Db.SaveChangesAsync();
        return record;
    }

    private static MultipartFormDataContent CreatePhotoMultipart(string fileName)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake image bytes"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", fileName);
        return content;
    }

    private async Task<(int ClientId, int PhotoId)> CreatePhotoForAnotherCoachInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");

        var coach = new Coach
        {
            Name = $"Photo Coach {suffix}",
            Specialty = "General",
            TenantId = 10
        };
        Db.Coaches.Add(coach);
        await Db.SaveChangesAsync();

        var client = new Client
        {
            FullName = $"Photo Client {suffix}",
            Email = $"photo-client-{suffix}@test.local",
            CoachId = coach.Id,
            TenantId = 10
        };
        Db.Clients.Add(client);
        await Db.SaveChangesAsync();

        var photo = new ClientProgressPhoto
        {
            ClientId = client.Id,
            PhotoUrl = "/uploads/progress/existing.jpg",
            PhotoType = "Front",
            TakenAt = DateTime.UtcNow,
            Notes = "Other coach photo"
        };
        Db.ClientProgressPhotos.Add(photo);
        await Db.SaveChangesAsync();

        return (client.Id, photo.Id);
    }
}
