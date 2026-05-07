using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<Exercise>> SearchAsync(string? q, string? category, int page, int pageSize, bool includeAll, int? coachId)
    {
        var query = _context.Exercises.AsNoTracking().AsQueryable();
        query = ApplyVisibility(query, includeAll, coachId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLower();
            query = query.Where(e => e.Category.ToLower() == cat);
        }

        return await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? q, string? category, bool includeAll, int? coachId)
    {
        var query = _context.Exercises.AsQueryable();
        query = ApplyVisibility(query, includeAll, coachId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim().ToLower();
            query = query.Where(e => e.Category.ToLower() == cat);
        }

        return await query.CountAsync();
    }

    private static IQueryable<Exercise> ApplyVisibility(IQueryable<Exercise> query, bool includeAll, int? coachId)
    {
        if (includeAll)
        {
            return query;
        }

        if (coachId.HasValue)
        {
            return query.Where(e => e.IsGlobal || e.CoachId == coachId.Value);
        }

        return query.Where(e => e.IsGlobal);
    }
}
