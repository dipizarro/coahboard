using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IEnumerable<Exercise>> SearchAsync(string? q, string? category, int page, int pageSize);
    Task<int> CountAsync(string? q, string? category);
}
