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
}
