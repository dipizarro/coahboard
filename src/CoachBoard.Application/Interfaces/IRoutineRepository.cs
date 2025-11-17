using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IRoutineRepository : IRepository<Routine>
{
    Task<Routine?> GetWithItemsAsync(int id);
    Task<IEnumerable<Routine>> GetByClientAsync(int clientId, int page, int pageSize, string? q);
    Task<int> CountByClientAsync(int clientId, string? q);

    Task ReplaceItemsAsync(int routineId, IEnumerable<RoutineExercise> items);
}
