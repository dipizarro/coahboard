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
            // Remove existing DbContextOptions
            services.RemoveAll<DbContextOptions<CoachBoardDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<CoachBoardDbContext>();
            
            Console.WriteLine("Removed DbContext services");

            // Create options for InMemory DB
            var options = new DbContextOptionsBuilder<CoachBoardDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            // Register options explicitly to bypass checking IConfigureOptions (which would include the Program.cs SQL Server config)
            services.AddSingleton(options);

            // Register the context using these options
            services.AddScoped<CoachBoardDbContext>();

            // If we need Seeding, we must do it within a scope
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
