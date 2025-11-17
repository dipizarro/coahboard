using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<IEnumerable<Client>> GetByCoachAsync(int coachId, int page, int pageSize, string? q);
    Task<int> CountByCoachAsync(int coachId, string? q);
}
