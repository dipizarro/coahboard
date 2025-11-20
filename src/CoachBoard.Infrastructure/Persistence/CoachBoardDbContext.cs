using CoachBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoachBoard.Infrastructure.Persistence;

public class CoachBoardDbContext : DbContext
{
    public CoachBoardDbContext(DbContextOptions<CoachBoardDbContext> options) : base(options) { }

    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineExercise> RoutineExercises => Set<RoutineExercise>();
    public DbSet<Session> Sessions => Set<Session>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coach>(e =>
        {
            e.ToTable("Coaches");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Specialty).IsRequired().HasMaxLength(80);

            // NUEVO: FK opcional hacia User
            e.HasOne(x => x.User)
                .WithMany() // no definimos colección en User (no es necesario)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).IsRequired().HasMaxLength(30);
        });

        modelBuilder.Entity<Client>(e =>
        {
            e.ToTable("Clients");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.HasOne(x => x.Coach)
                .WithMany(c => c.Clients)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.CoachId, x.FullName });
        });

        // Exercise
        modelBuilder.Entity<Exercise>(e =>
        {
            e.ToTable("Exercises");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Category).IsRequired().HasMaxLength(60);
        });

        // Routine
        modelBuilder.Entity<Routine>(e =>
        {
            e.ToTable("Routines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            e.HasOne(x => x.Client)
                .WithMany(c => c.Routines)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RoutineExercise (join con metadata)
        modelBuilder.Entity<RoutineExercise>(e =>
        {
            e.ToTable("RoutineExercises");
            e.HasKey(x => new { x.RoutineId, x.ExerciseId, x.Order }); // compuesta para permitir repetir ejercicio en distinto orden

            e.HasOne(x => x.Routine)
                .WithMany(r => r.RoutineExercises)
                .HasForeignKey(x => x.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Exercise)
                .WithMany(ex => ex.RoutineExercises)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Sets).IsRequired();
            e.Property(x => x.Reps).IsRequired();
            e.Property(x => x.Order).IsRequired();
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.Id);

            e.Property(x => x.StartAt).IsRequired();
            e.Property(x => x.EndAt).IsRequired();

            e.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(30);

            e.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(30);

            e.Property(x => x.Location)
                .HasMaxLength(150);

            e.Property(x => x.Notes)
                .HasMaxLength(500);

            e.HasOne(x => x.Coach)
                .WithMany(c => c.Sessions)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.Restrict);   // ⬅ ya lo tenías NO ACTION

            e.HasOne(x => x.Client)
                .WithMany(c => c.Sessions)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);   // ⬅ CAMBIAR SetNull → Restrict

            e.HasOne(x => x.Routine)
                .WithMany(r => r.Sessions)
                .HasForeignKey(x => x.RoutineId)
                .OnDelete(DeleteBehavior.Restrict);   // ⬅ CAMBIAR SetNull → Restrict

            e.HasIndex(x => new { x.CoachId, x.StartAt });
        });


        base.OnModelCreating(modelBuilder);
    }
}
