using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class ClientProgressPhotoRepository : Repository<ClientProgressPhoto>, IClientProgressPhotoRepository
{
    public ClientProgressPhotoRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<ClientProgressPhoto>> GetByClientAsync(int clientId)
    {
        return await _context.ClientProgressPhotos
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.TakenAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<ClientProgressPhoto?> GetByClientAndIdAsync(int clientId, int photoId)
    {
        return await _context.ClientProgressPhotos
            .FirstOrDefaultAsync(x => x.ClientId == clientId && x.Id == photoId);
    }
}
