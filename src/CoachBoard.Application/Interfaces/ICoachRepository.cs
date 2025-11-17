using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface ICoachRepository : IRepository<Coach>
{
    Task<IEnumerable<Coach>> GetBySpecialtyAsync(string specialty);
    Task<Coach?> GetByUserIdAsync(int userId);
}
