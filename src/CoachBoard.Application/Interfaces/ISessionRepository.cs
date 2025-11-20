using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface ISessionRepository : IRepository<Session>
{
    Task<IEnumerable<Session>> GetByCoachAsync(
        int coachId,
        DateTime from,
        DateTime to,
        int? clientId);

    Task<Session?> GetWithRelationsAsync(int id);
}
