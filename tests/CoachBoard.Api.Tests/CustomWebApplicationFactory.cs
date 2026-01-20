using CoachBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoachBoard.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _databaseName = $"CoachBoardTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=CoachBoardTest;Trusted_Connection=True;",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Key"] = "TestKey-TestKey-TestKey-TestKey-TestKey"
            };

            config.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CoachBoardDbContext>>();
            services.AddDbContext<CoachBoardDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(CoachBoardDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        var user = new Domain.Entities.User
        {
            Id = 1,
            Email = "coach@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
            Role = "Coach"
        };

        var coach = new Domain.Entities.Coach
        {
            Id = 1,
            UserId = user.Id,
            Name = "Test Coach",
            Specialty = "General"
        };

        db.Users.Add(user);
        db.Coaches.Add(coach);
        db.SaveChanges();
    }
}
