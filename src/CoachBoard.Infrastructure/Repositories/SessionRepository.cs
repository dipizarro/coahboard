using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<Session>> GetByCoachAsync(
        int coachId,
        DateTime from,
        DateTime to,
        int? clientId)
    {
        var query = GetQuery()
            .Include(s => s.Client)
            .Include(s => s.Routine)
            .AsNoTracking()
            .Where(s => s.CoachId == coachId &&
                        s.StartAt >= from &&
                        s.StartAt <= to);

        if (clientId.HasValue && clientId.Value > 0)
        {
            query = query.Where(s => s.ClientId == clientId.Value);
        }

        return await query
            .OrderBy(s => s.StartAt)
            .ToListAsync();
    }

    public async Task<Session?> GetWithRelationsAsync(int id)
    {
        return await GetQuery()
            .Include(s => s.Client)
            .Include(s => s.Routine)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
