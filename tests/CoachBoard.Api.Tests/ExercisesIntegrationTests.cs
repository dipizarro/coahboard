using System.Net;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
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
        created.IsActive.Should().BeTrue();

        var update = new ExerciseUpdateDto(
            Name: "Sentadilla goblet pausada",
            Category: "Fuerza",
            DefaultSets: 3,
            DefaultReps: 8,
            Description: "Sentadilla goblet con pausa abajo.",
            Instructions: "Pausar dos segundos en el punto bajo antes de subir.",
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
        updated.DifficultyLevel.Should().Be("Avanzado");
        updated.Equipment.Should().Be("Kettlebell");
        updated.IsActive.Should().BeFalse();

        var getResponse = await Client.GetAsync($"/api/exercises/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ExerciseReadDto>();
        fetched!.IsActive.Should().BeFalse();
        fetched.Tags.Should().Be(update.Tags);
    }
}
