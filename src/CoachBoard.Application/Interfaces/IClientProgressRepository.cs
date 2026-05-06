using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IClientProgressRepository : IRepository<ClientProgressRecord>
{
    Task<IEnumerable<ClientProgressRecord>> GetByClientAsync(int clientId);
    Task<ClientProgressRecord?> GetByClientAndIdAsync(int clientId, int progressId);
}
