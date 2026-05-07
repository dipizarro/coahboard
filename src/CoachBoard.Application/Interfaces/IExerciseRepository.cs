using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IEnumerable<Exercise>> SearchAsync(
        string? q,
        string? category,
        string? targetMuscleGroup,
        string? equipment,
        string? difficultyLevel,
        string? exerciseType,
        string? environment,
        string? tag,
        int page,
        int pageSize,
        bool includeAll,
        int? coachId);

    Task<int> CountAsync(
        string? q,
        string? category,
        string? targetMuscleGroup,
        string? equipment,
        string? difficultyLevel,
        string? exerciseType,
        string? environment,
        string? tag,
        bool includeAll,
        int? coachId);
}
