using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(CoachBoardDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
}
