using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class ExerciseMediaRepository : Repository<ExerciseMedia>, IExerciseMediaRepository
{
    public ExerciseMediaRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<ExerciseMedia>> GetByExerciseAsync(int exerciseId)
    {
        return await _context.ExerciseMedia
            .AsNoTracking()
            .Where(x => x.ExerciseId == exerciseId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<ExerciseMedia?> GetByExerciseAndIdAsync(int exerciseId, int mediaId)
    {
        return await _context.ExerciseMedia
            .FirstOrDefaultAsync(x => x.ExerciseId == exerciseId && x.Id == mediaId);
    }
}
