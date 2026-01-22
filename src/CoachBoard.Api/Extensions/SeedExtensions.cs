using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using BCrypt.Net;

namespace CoachBoard.Api.Extensions;

public static class SeedExtensions
{
    public static async Task SeedAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var admin = await users.GetByEmailAsync("admin@coachboard.cl");
        if (admin is null)
        {
            await users.AddAsync(new User
            {
                Email = "admin@coachboard.cl",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234!", 11),
                Role = "Admin"
            });
            await users.SaveChangesAsync();
        }

    }

    public static async Task SeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var exercises = scope.ServiceProvider.GetRequiredService<IExerciseRepository>();
        if (!(await exercises.GetAllAsync()).Any())
        {
            await exercises.AddAsync(new Exercise { Name = "Sentadilla", Category = "Fuerza", DefaultSets = 4, DefaultReps = 8 });
            await exercises.AddAsync(new Exercise { Name = "Press banca", Category = "Fuerza", DefaultSets = 4, DefaultReps = 10 });
            await exercises.AddAsync(new Exercise { Name = "Remo con mancuerna", Category = "Fuerza", DefaultSets = 3, DefaultReps = 12 });
            await exercises.SaveChangesAsync();
        }
    }
}
