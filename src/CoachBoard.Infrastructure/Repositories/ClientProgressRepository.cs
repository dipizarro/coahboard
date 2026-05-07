using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class ClientProgressRepository : Repository<ClientProgressRecord>, IClientProgressRepository
{
    public ClientProgressRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<ClientProgressRecord>> GetByClientAsync(int clientId)
    {
        return await _context.ClientProgressRecords
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<ClientProgressRecord?> GetByClientAndIdAsync(int clientId, int progressId)
    {
        return await _context.ClientProgressRecords
            .FirstOrDefaultAsync(x => x.ClientId == clientId && x.Id == progressId);
    }

    public async Task<(ClientProgressRecord? First, ClientProgressRecord? Last, int Total)> GetFirstLastAndCountByClientAsync(int clientId)
    {
        var query = _context.ClientProgressRecords
            .AsNoTracking()
            .Where(x => x.ClientId == clientId);

        var total = await query.CountAsync();
        if (total == 0)
        {
            return (null, null, 0);
        }

        var first = await query
            .OrderBy(x => x.RecordedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync();

        var last = await query
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        return (first, last, total);
    }
}
