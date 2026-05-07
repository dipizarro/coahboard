using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IEnumerable<Exercise>> SearchAsync(string? q, string? category, int page, int pageSize, bool includeAll, int? coachId);
    Task<int> CountAsync(string? q, string? category, bool includeAll, int? coachId);
}
