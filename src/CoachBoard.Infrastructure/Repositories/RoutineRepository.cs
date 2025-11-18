using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class RoutineRepository : Repository<Routine>, IRoutineRepository
{
    public RoutineRepository(CoachBoardDbContext context) : base(context) { }

    public async Task<Routine?> GetWithItemsAsync(int id)
    {
        return await _context.Routines
            .Include(r => r.RoutineExercises)
                .ThenInclude(re => re.Exercise)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Routine>> GetByClientAsync(int clientId, int page, int pageSize, string? q)
    {
        // Declarar explícitamente como IQueryable para poder reasignar después de Where/Include
        IQueryable<Routine> query = _context.Routines
            .AsNoTracking()
            .Where(r => r.ClientId == clientId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term));
        }

        // Incluir relaciones (items y ejercicios) antes de materializar la consulta
        query = query
            .Include(r => r.RoutineExercises)
                .ThenInclude(re => re.Exercise);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByClientAsync(int clientId, string? q)
    {
        var query = _context.Routines.Where(r => r.ClientId == clientId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term));
        }
        return await query.CountAsync();
    }

    public async Task ReplaceItemsAsync(int routineId, IEnumerable<RoutineExercise> items)
    {
        var current = await _context.RoutineExercises.Where(x => x.RoutineId == routineId).ToListAsync();
        _context.RoutineExercises.RemoveRange(current);
        await _context.RoutineExercises.AddRangeAsync(items);
    }
}