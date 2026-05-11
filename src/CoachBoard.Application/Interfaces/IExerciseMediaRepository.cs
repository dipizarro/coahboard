using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IExerciseMediaRepository : IRepository<ExerciseMedia>
{
    Task<IEnumerable<ExerciseMedia>> GetByExerciseAsync(int exerciseId);
    Task<ExerciseMedia?> GetByExerciseAndIdAsync(int exerciseId, int mediaId);
}
