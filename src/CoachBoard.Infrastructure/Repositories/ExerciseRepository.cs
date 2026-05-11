using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Repositories;

public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(CoachBoardDbContext context, ICurrentTenant currentTenant) : base(context, currentTenant) { }

    public async Task<IEnumerable<Exercise>> SearchAsync(
        string? q,
        string? category,
        string? targetMuscleGroup,
        string? equipment,
        string? difficultyLevel,
        string? exerciseType,
        string? environment,
        string? tag,
        int page,
        int pageSize,
        bool includeAll,
        int? coachId)
    {
        var query = _context.Exercises.AsNoTracking().AsQueryable();
        query = ApplyVisibility(query, includeAll, coachId);
        query = ApplyFilters(query, q, category, targetMuscleGroup, equipment, difficultyLevel, exerciseType, environment, tag);

        var exercises = await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        await FillMissingImageUrlsFromMediaAsync(exercises);
        return exercises;
    }

    public async Task<int> CountAsync(
        string? q,
        string? category,
        string? targetMuscleGroup,
        string? equipment,
        string? difficultyLevel,
        string? exerciseType,
        string? environment,
        string? tag,
        bool includeAll,
        int? coachId)
    {
        var query = _context.Exercises.AsQueryable();
        query = ApplyVisibility(query, includeAll, coachId);
        query = ApplyFilters(query, q, category, targetMuscleGroup, equipment, difficultyLevel, exerciseType, environment, tag);

        return await query.CountAsync();
    }

    private static IQueryable<Exercise> ApplyFilters(
        IQueryable<Exercise> query,
        string? q,
        string? category,
        string? targetMuscleGroup,
        string? equipment,
        string? difficultyLevel,
        string? exerciseType,
        string? environment,
        string? tag)
    {
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term)
                || (e.Description != null && e.Description.ToLower().Contains(term))
                || (e.Tags != null && e.Tags.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var value = category.Trim().ToLower();
            query = query.Where(e => e.Category.ToLower() == value);
        }

        if (!string.IsNullOrWhiteSpace(targetMuscleGroup))
        {
            var value = targetMuscleGroup.Trim().ToLower();
            query = query.Where(e => e.TargetMuscleGroup != null && e.TargetMuscleGroup.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(equipment))
        {
            var value = equipment.Trim().ToLower();
            query = query.Where(e => e.Equipment != null && e.Equipment.ToLower().Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(difficultyLevel))
        {
            var value = difficultyLevel.Trim().ToLower();
            query = query.Where(e => e.DifficultyLevel != null && e.DifficultyLevel.ToLower() == value);
        }

        if (!string.IsNullOrWhiteSpace(exerciseType))
        {
            var value = exerciseType.Trim().ToLower();
            query = query.Where(e => e.ExerciseType != null && e.ExerciseType.ToLower() == value);
        }

        if (!string.IsNullOrWhiteSpace(environment))
        {
            var value = environment.Trim().ToLower();
            query = query.Where(e => e.Environment != null && e.Environment.ToLower() == value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var value = tag.Trim().ToLower();
            query = query.Where(e => e.Tags != null && e.Tags.ToLower().Contains(value));
        }

        return query;
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

    private async Task FillMissingImageUrlsFromMediaAsync(List<Exercise> exercises)
    {
        var exerciseIds = exercises
            .Where(x => string.IsNullOrWhiteSpace(x.ImageUrl))
            .Select(x => x.Id)
            .ToList();

        if (exerciseIds.Count == 0) return;

        var media = await _context.ExerciseMedia
            .AsNoTracking()
            .Where(x => exerciseIds.Contains(x.ExerciseId))
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new { x.ExerciseId, x.Url })
            .ToListAsync();

        var firstMediaByExercise = media
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(x => x.Key, x => x.First().Url);

        foreach (var exercise in exercises)
        {
            if (firstMediaByExercise.TryGetValue(exercise.Id, out var imageUrl))
            {
                exercise.ImageUrl = imageUrl;
            }
        }
    }
}
