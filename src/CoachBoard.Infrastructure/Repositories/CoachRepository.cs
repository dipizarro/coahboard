using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class CoachRepository : Repository<Coach>, ICoachRepository
{
    public CoachRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<Coach>> GetBySpecialtyAsync(string specialty)
    {
        return await GetQuery()
            .Where(c => c.Specialty == specialty)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Coach?> GetByUserIdAsync(int userId)
    {
        return await _context.Coaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
