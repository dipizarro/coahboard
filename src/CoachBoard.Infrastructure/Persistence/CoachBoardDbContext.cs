using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
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
    public DbSet<ClientProgressRecord> ClientProgressRecords => Set<ClientProgressRecord>();
    public DbSet<ClientProgressPhoto> ClientProgressPhotos => Set<ClientProgressPhoto>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tenant
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.Property(x => x.Plan)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(SubscriptionPlan.Free);
            e.HasData(new Tenant { Id = 1, Name = "Default Tenant", Plan = SubscriptionPlan.Free, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        });

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

            e.Property(x => x.TenantId).HasDefaultValue(1);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).IsRequired().HasMaxLength(30);

            e.Property(x => x.TenantId).HasDefaultValue(1);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Client>(e =>
        {
            e.ToTable("Clients");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Gender).HasMaxLength(30);
            e.Property(x => x.InitialHeightCm).HasColumnType("decimal(5,2)");
            e.Property(x => x.MainGoal).HasMaxLength(150);
            e.Property(x => x.ExperienceLevel).HasMaxLength(50);
            e.Property(x => x.MedicalNotes).HasMaxLength(1000);
            e.Property(x => x.InjuryNotes).HasMaxLength(1000);
            e.Property(x => x.GeneralNotes).HasMaxLength(1000);
            e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            e.HasOne(x => x.Coach)
                .WithMany(c => c.Clients)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.CoachId, x.FullName });

            e.Property(x => x.TenantId).HasDefaultValue(1);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientProgressRecord>(e =>
        {
            e.ToTable("ClientProgressRecords");
            e.HasKey(x => x.Id);
            e.Property(x => x.RecordedAt).IsRequired();
            e.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");
            e.Property(x => x.HeightCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.BodyFatPercentage).HasColumnType("decimal(6,2)");
            e.Property(x => x.ChestCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.WaistCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.HipCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.LeftArmCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.RightArmCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.LeftThighCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.RightThighCm).HasColumnType("decimal(6,2)");
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasOne(x => x.Client)
                .WithMany(c => c.ProgressRecords)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.ClientId, x.RecordedAt });
        });

        modelBuilder.Entity<ClientProgressPhoto>(e =>
        {
            e.ToTable("ClientProgressPhotos");
            e.HasKey(x => x.Id);
            e.Property(x => x.PhotoUrl).IsRequired().HasMaxLength(1000);
            e.Property(x => x.PhotoType).IsRequired().HasMaxLength(30);
            e.Property(x => x.TakenAt).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Client)
                .WithMany(c => c.ProgressPhotos)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ClientProgressRecord)
                .WithMany(r => r.Photos)
                .HasForeignKey(x => x.ClientProgressRecordId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => new { x.ClientId, x.TakenAt });
        });

        // Exercise
        modelBuilder.Entity<Exercise>(e =>
        {
            e.ToTable("Exercises");
            e.HasKey(x => x.Id);
            e.Property(x => x.IsGlobal).IsRequired().HasDefaultValue(false);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Category).IsRequired().HasMaxLength(60);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Instructions).HasMaxLength(2000);
            e.Property(x => x.VideoUrl).HasMaxLength(500);
            e.Property(x => x.ReferenceUrl).HasMaxLength(500);
            e.Property(x => x.DifficultyLevel).HasMaxLength(50);
            e.Property(x => x.MovementPattern).HasMaxLength(80);
            e.Property(x => x.Equipment).HasMaxLength(100);
            e.Property(x => x.TargetMuscleGroup).HasMaxLength(80);
            e.Property(x => x.SecondaryMuscleGroups).HasMaxLength(300);
            e.Property(x => x.ExerciseType).HasMaxLength(80);
            e.Property(x => x.Environment).HasMaxLength(80);
            e.Property(x => x.Tags).HasMaxLength(500);
            e.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            e.HasOne(x => x.Coach)
                .WithMany(c => c.Exercises)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.IsGlobal, x.CoachId });
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

            e.Property(x => x.TenantId).HasDefaultValue(1);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
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

            e.Property(x => x.TenantId).HasDefaultValue(1);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });


        // FeatureFlag
        modelBuilder.Entity<FeatureFlag>(e =>
        {
            e.ToTable("FeatureFlags");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.IsEnabled).IsRequired();

            e.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });






        // Subscription
        modelBuilder.Entity<Subscription>(e =>
        {
            e.ToTable("Subscriptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderSubscriptionId).IsRequired().HasMaxLength(100);
            e.Property(x => x.Status).IsRequired().HasConversion<string>();

            e.HasOne(x => x.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

		base.OnModelCreating(modelBuilder);
    }
}
