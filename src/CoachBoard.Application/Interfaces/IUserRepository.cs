using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
