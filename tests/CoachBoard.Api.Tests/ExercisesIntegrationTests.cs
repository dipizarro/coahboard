using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class ExercisesIntegrationTests : BaseIntegrationTest
{
    public ExercisesIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ExerciseCrud_WithAdvancedFields_PersistsAndReturnsFields()
    {
        await AuthenticateAsUserAAsync();

        var create = new ExerciseCreateDto(
            Name: "Sentadilla goblet",
            Category: "Fuerza",
            DefaultSets: 4,
            DefaultReps: 10,
            Description: "Variante de sentadilla con carga frontal.",
            Instructions: "Sostener la mancuerna al pecho y descender con control.",
            ImageUrl: "https://example.com/images/goblet-squat.jpg",
            VideoUrl: "https://example.com/videos/goblet-squat",
            ReferenceUrl: "https://example.com/exercises/goblet-squat",
            DifficultyLevel: "Intermedio",
            MovementPattern: "Squat",
            Equipment: "Mancuerna",
            TargetMuscleGroup: "Cuádriceps",
            SecondaryMuscleGroups: "Glúteos, core",
            ExerciseType: "Fuerza",
            Environment: "Gimnasio",
            Tags: "piernas,sentadilla,mancuerna");

        var createResponse = await Client.PostAsJsonAsync("/api/exercises", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        created.Should().NotBeNull();
        created!.Name.Should().Be(create.Name);
        created.Description.Should().Be(create.Description);
        created.Instructions.Should().Be(create.Instructions);
        created.ImageUrl.Should().Be(create.ImageUrl);
        created.VideoUrl.Should().Be(create.VideoUrl);
        created.ReferenceUrl.Should().Be(create.ReferenceUrl);
        created.DifficultyLevel.Should().Be(create.DifficultyLevel);
        created.MovementPattern.Should().Be(create.MovementPattern);
        created.Equipment.Should().Be(create.Equipment);
        created.TargetMuscleGroup.Should().Be(create.TargetMuscleGroup);
        created.SecondaryMuscleGroups.Should().Be(create.SecondaryMuscleGroups);
        created.ExerciseType.Should().Be(create.ExerciseType);
        created.Environment.Should().Be(create.Environment);
        created.Tags.Should().Be(create.Tags);
        created.CoachId.Should().Be(10);
        created.IsGlobal.Should().BeFalse();
        created.IsActive.Should().BeTrue();

        var update = new ExerciseUpdateDto(
            Name: "Sentadilla goblet pausada",
            Category: "Fuerza",
            DefaultSets: 3,
            DefaultReps: 8,
            Description: "Sentadilla goblet con pausa abajo.",
            Instructions: "Pausar dos segundos en el punto bajo antes de subir.",
            ImageUrl: "https://example.com/images/goblet-squat-paused.webp",
            DifficultyLevel: "Avanzado",
            MovementPattern: "Squat",
            Equipment: "Kettlebell",
            TargetMuscleGroup: "Cuádriceps",
            SecondaryMuscleGroups: "Glúteos, aductores, core",
            ExerciseType: "Fuerza",
            Environment: "Gimnasio",
            Tags: "piernas,sentadilla,pausa",
            IsActive: false);

        var updateResponse = await Client.PutAsJsonAsync($"/api/exercises/{created.Id}", update);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(update.Name);
        updated.DefaultSets.Should().Be(3);
        updated.DefaultReps.Should().Be(8);
        updated.Description.Should().Be(update.Description);
        updated.Instructions.Should().Be(update.Instructions);
        updated.ImageUrl.Should().Be(update.ImageUrl);
        updated.DifficultyLevel.Should().Be("Avanzado");
        updated.Equipment.Should().Be("Kettlebell");
        updated.IsActive.Should().BeFalse();

        var getResponse = await Client.GetAsync($"/api/exercises/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        fetched!.IsActive.Should().BeFalse();
        fetched.Tags.Should().Be(update.Tags);
        fetched.CoachId.Should().Be(10);
        fetched.IsGlobal.Should().BeFalse();
    }

    [Fact]
    public async Task Search_WhenCoachAuthenticated_ReturnsGlobalAndOwnExercisesOnly()
    {
        var ownExercise = new Exercise { Name = "Own Coach Exercise", Category = "Fuerza", CoachId = 10, IsGlobal = false };
        var otherExercise = await CreateExerciseForAnotherCoachInTenantAAsync();
        Db.Exercises.Add(ownExercise);
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        var response = await Client.GetAsync("/api/exercises?page=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ExerciseReadDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().Contain(x => x.Id == 10 && x.IsGlobal);
        result.Items.Should().Contain(x => x.Id == ownExercise.Id && x.CoachId == 10 && !x.IsGlobal);
        result.Items.Should().NotContain(x => x.Id == otherExercise.Id);
    }

    [Fact]
    public async Task Search_WhenAnonymous_ReturnsGlobalExercisesOnly()
    {
        Db.Exercises.Add(new Exercise { Name = "Private Anonymous Hidden", Category = "Fuerza", CoachId = 10, IsGlobal = false });
        await Db.SaveChangesAsync();

        var response = await Client.GetAsync("/api/exercises?page=1&pageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ExerciseReadDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().Contain(x => x.Id == 10 && x.IsGlobal);
        result.Items.Should().OnlyContain(x => x.IsGlobal);
    }

    [Fact]
    public async Task ExerciseEndpoints_WhenCoachAccessesAnotherCoachExercise_ReturnForbiddenOrNotFound()
    {
        var otherExercise = await CreateExerciseForAnotherCoachInTenantAAsync();
        await AuthenticateAsUserAAsync();

        var getResponse = await Client.GetAsync($"/api/exercises/{otherExercise.Id}");
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/exercises/{otherExercise.Id}",
            new ExerciseUpdateDto("Updated", "Fuerza", 3, 10));
        var deleteResponse = await Client.DeleteAsync($"/api/exercises/{otherExercise.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExerciseEndpoints_WhenCoachEditsGlobalExercise_ReturnForbidden()
    {
        await AuthenticateAsUserAAsync();

        var updateResponse = await Client.PutAsJsonAsync(
            "/api/exercises/10",
            new ExerciseUpdateDto("Push Up Updated", "Chest", 3, 15));
        var deleteResponse = await Client.DeleteAsync("/api/exercises/10");

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExerciseEndpoints_WhenAdminCreatesGlobalOrAssignedExercise_Succeeds()
    {
        await AuthenticateAsAdminInTenantAAsync();

        var globalCreate = new ExerciseCreateDto(
            Name: "Admin Global Exercise",
            Category: "General",
            DefaultSets: null,
            DefaultReps: null,
            IsGlobal: true,
            CoachId: 10);
        var assignedCreate = new ExerciseCreateDto(
            Name: "Admin Assigned Exercise",
            Category: "Fuerza",
            DefaultSets: 3,
            DefaultReps: 12,
            CoachId: 10);

        var globalResponse = await Client.PostAsJsonAsync("/api/exercises", globalCreate);
        var assignedResponse = await Client.PostAsJsonAsync("/api/exercises", assignedCreate);

        globalResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        assignedResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var global = await globalResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        var assigned = await assignedResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        global!.IsGlobal.Should().BeTrue();
        global.CoachId.Should().BeNull();
        assigned!.IsGlobal.Should().BeFalse();
        assigned.CoachId.Should().Be(10);
    }

    [Fact]
    public async Task Search_WithAdvancedFilters_ReturnsMatchingVisibleExercises()
    {
        Db.Exercises.AddRange(
            new Exercise
            {
                Name = "Press inclinado con mancuernas",
                Category = "Fuerza",
                CoachId = 10,
                IsGlobal = false,
                Description = "Trabajo de empuje para pectoral superior.",
                DifficultyLevel = "Intermedio",
                Equipment = "Mancuernas",
                TargetMuscleGroup = "Pectoral",
                ExerciseType = "Fuerza",
                Environment = "Gimnasio",
                Tags = "empuje,pecho,hipertrofia"
            },
            new Exercise
            {
                Name = "Press en máquina",
                Category = "Fuerza",
                CoachId = 10,
                IsGlobal = false,
                Description = "Empuje guiado.",
                DifficultyLevel = "Inicial",
                Equipment = "Máquina",
                TargetMuscleGroup = "Pectoral",
                ExerciseType = "Fuerza",
                Environment = "Gimnasio",
                Tags = "pecho"
            },
            new Exercise
            {
                Name = "Zancadas al aire libre",
                Category = "Fuerza",
                CoachId = 10,
                IsGlobal = false,
                DifficultyLevel = "Inicial",
                Equipment = "Peso corporal",
                TargetMuscleGroup = "Piernas",
                ExerciseType = "Fuerza",
                Environment = "Exterior",
                Tags = "piernas"
            });
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        var response = await Client.GetAsync("/api/exercises?q=hipertrofia&category=Fuerza&targetMuscleGroup=pectoral&equipment=mancuerna&difficultyLevel=Intermedio&exerciseType=Fuerza&environment=Gimnasio&tag=empuje&page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ExerciseReadDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Press inclinado con mancuernas");
    }

    [Fact]
    public async Task Search_WhenQMatchesDescriptionOrTags_ReturnsExercise()
    {
        Db.Exercises.Add(new Exercise
        {
            Name = "Curl alterno",
            Category = "Fuerza",
            CoachId = 10,
            IsGlobal = false,
            Description = "Trabajo estricto de flexión de codo.",
            Tags = "brazo,biceps"
        });
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        var descriptionResponse = await Client.GetAsync("/api/exercises?q=codo&page=1&pageSize=20");
        var tagsResponse = await Client.GetAsync("/api/exercises?q=biceps&page=1&pageSize=20");

        descriptionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tagsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var descriptionResult = await descriptionResponse.Content.ReadFromJsonAsync<PagedResult<ExerciseReadDto>>();
        var tagsResult = await tagsResponse.Content.ReadFromJsonAsync<PagedResult<ExerciseReadDto>>();
        descriptionResult!.Items.Should().Contain(x => x.Name == "Curl alterno");
        tagsResult!.Items.Should().Contain(x => x.Name == "Curl alterno");
    }

    [Fact]
    public async Task ExerciseMediaUploadListAndDelete_ForOwnExercise_Succeeds()
    {
        var exercise = new Exercise { Name = "Media Own Exercise", Category = "Fuerza", CoachId = 10, IsGlobal = false };
        Db.Exercises.Add(exercise);
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        var upload = CreateImageMultipart("exercise.jpg");
        upload.Add(new StringContent("Vista frontal"), "title");
        upload.Add(new StringContent("Referencia visual"), "description");

        var createResponse = await Client.PostAsync($"/api/exercises/{exercise.Id}/media", upload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ExerciseMediaReadDto>();
        created.Should().NotBeNull();
        created!.ExerciseId.Should().Be(exercise.Id);
        created.MediaType.Should().Be("Image");
        created.Url.Should().Contain("/uploads/");
        created.Title.Should().Be("Vista frontal");

        var listResponse = await Client.GetAsync($"/api/exercises/{exercise.Id}/media");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var media = await listResponse.Content.ReadFromJsonAsync<List<ExerciseMediaReadDto>>();
        media.Should().ContainSingle(x => x.Id == created.Id);

        var deleteResponse = await Client.DeleteAsync($"/api/exercises/{exercise.Id}/media/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDeleteResponse = await Client.GetAsync($"/api/exercises/{exercise.Id}/media");
        var afterDelete = await afterDeleteResponse.Content.ReadFromJsonAsync<List<ExerciseMediaReadDto>>();
        afterDelete.Should().NotContain(x => x.Id == created.Id);
    }

    [Fact]
    public async Task ExerciseMediaUpload_WithInvalidExtension_ReturnsBadRequest()
    {
        var exercise = new Exercise { Name = "Invalid Media Exercise", Category = "Fuerza", CoachId = 10, IsGlobal = false };
        Db.Exercises.Add(exercise);
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        var response = await Client.PostAsync($"/api/exercises/{exercise.Id}/media", CreateImageMultipart("exercise.gif"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExerciseMediaEndpoints_WhenCoachAccessesAnotherCoachExercise_ReturnForbiddenOrNotFound()
    {
        var otherExercise = await CreateExerciseForAnotherCoachInTenantAAsync();
        var media = new ExerciseMedia
        {
            ExerciseId = otherExercise.Id,
            MediaType = "Image",
            Url = "/uploads/progress/existing.jpg"
        };
        Db.ExerciseMedia.Add(media);
        await Db.SaveChangesAsync();

        await AuthenticateAsUserAAsync();

        (await Client.GetAsync($"/api/exercises/{otherExercise.Id}/media")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.PostAsync($"/api/exercises/{otherExercise.Id}/media", CreateImageMultipart("blocked.jpg"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await Client.DeleteAsync($"/api/exercises/{otherExercise.Id}/media/{media.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Exercise> CreateExerciseForAnotherCoachInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");

        var coach = new Coach
        {
            Name = $"Exercise Coach {suffix}",
            Specialty = "General",
            TenantId = 10
        };
        Db.Coaches.Add(coach);
        await Db.SaveChangesAsync();

        var exercise = new Exercise
        {
            Name = $"Other Coach Exercise {suffix}",
            Category = "Fuerza",
            CoachId = coach.Id,
            IsGlobal = false
        };
        Db.Exercises.Add(exercise);
        await Db.SaveChangesAsync();

        return exercise;
    }

    private static MultipartFormDataContent CreateImageMultipart(string fileName)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([0x01, 0x02, 0x03, 0x04]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", fileName);
        return content;
    }

    private async Task AuthenticateAsAdminInTenantAAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"exercise-admin-{suffix}@test.local";
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
