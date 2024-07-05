using DataLibrary.ModelsV2;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Context;

public partial class TrainingLogV2Context : DbContext
{
    public TrainingLogV2Context()
    {
    }

    public TrainingLogV2Context(DbContextOptions<TrainingLogV2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<BlockExercise> BlockExercises { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ExerciseHowTo> ExerciseHowTos { get; set; }

    public virtual DbSet<ExerciseMuscle> ExerciseMuscles { get; set; }

    public virtual DbSet<ExerciseRecord> ExerciseRecords { get; set; }

    public virtual DbSet<Muscle> Muscles { get; set; }

    public virtual DbSet<TrainingDay> TrainingDays { get; set; }

    public virtual DbSet<TrainingPlan> TrainingPlans { get; set; }

    public virtual DbSet<TrainingSession> TrainingSessions { get; set; }

    public virtual DbSet<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; }

    public virtual DbSet<TrainingType> TrainingTypes { get; set; }

    public virtual DbSet<TrainingWeek> TrainingWeeks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlite("Data Source=E:/development/databases/training_log_v2.db");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Block>(entity =>
        {
            entity.ToTable("block");

            entity.HasIndex(e => e.Id, "idx_block_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Instrcustions).HasColumnName("instrcustions");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.RestInSeconds).HasColumnName("rest_in_seconds");
            entity.Property(e => e.Sets).HasColumnName("sets");
            entity.Property(e => e.TrainingDayId).HasColumnName("training_day_id");

            entity.HasOne(d => d.TrainingDay).WithMany(p => p.Blocks)
                .HasForeignKey(d => d.TrainingDayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlockExercise>(entity =>
        {
            entity.ToTable("block_exercises");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DistanceInMeters).HasColumnName("distance_in_meters");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.Repetitions).HasColumnName("repetitions");
            entity.Property(e => e.TimerInSeconds).HasColumnName("timer_in_seconds");

            entity.HasOne(d => d.Block).WithMany(p => p.BlockExercises)
                .HasForeignKey(d => d.BlockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Exercise).WithMany(p => p.BlockExercises)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("equipment");

            entity.HasIndex(e => e.Id, "idx_equipment_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercise");

            entity.HasIndex(e => e.Name, "IX_exercise_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Difficulty)
                .HasDefaultValue(0)
                .HasColumnName("difficulty");
            entity.Property(e => e.HowTo).HasColumnName("how_to");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");

            entity.HasMany(d => d.TrainingTypes).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseType",
                    r => r.HasOne<TrainingType>().WithMany().HasForeignKey("TrainingTypeId"),
                    l => l.HasOne<Exercise>().WithMany().HasForeignKey("ExerciseId"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "TrainingTypeId");
                        j.ToTable("exercise_type");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("exercise_id");
                        j.IndexerProperty<int>("TrainingTypeId").HasColumnName("training_type_id");
                    });
        });

        modelBuilder.Entity<ExerciseHowTo>(entity =>
        {
            entity.ToTable("exercise_how_to");

            entity.HasIndex(e => e.ExerciseId, "idx_exercise_how_to_exercise_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Url)
                .HasColumnType("varchar(255)")
                .HasColumnName("url");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseHowTos)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExerciseMuscle>(entity =>
        {
            entity.HasKey(e => new { e.MuscleId, e.ExerciseId });

            entity.ToTable("exercise_muscle");

            entity.HasIndex(e => e.IsPrimary, "idx_exercise_muscle_is_primary");

            entity.Property(e => e.MuscleId).HasColumnName("muscle_id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValueSql("false")
                .HasColumnType("boolean")
                .HasColumnName("is_primary");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseMuscles).HasForeignKey(d => d.ExerciseId);

            entity.HasOne(d => d.Muscle).WithMany(p => p.ExerciseMuscles).HasForeignKey(d => d.MuscleId);
        });

        modelBuilder.Entity<ExerciseRecord>(entity =>
        {
            entity.ToTable("exercise_record");

            entity.HasIndex(e => e.CreatedAt, "idx_exercise_record_created_at");

            entity.HasIndex(e => e.Id, "idx_exercise_record_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DistanceInMeters).HasColumnName("distance_in_meters");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Repetitions).HasColumnName("repetitions");
            entity.Property(e => e.TimerInSeconds).HasColumnName("timer_in_seconds");
            entity.Property(e => e.WeightUsedKg).HasColumnName("weight_used_kg");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseRecords)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.ToTable("muscle");

            entity.HasIndex(e => e.Name, "IX_muscle_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Function).HasColumnName("function");
            entity.Property(e => e.MuscleGroup)
                .HasColumnType("varchar(64)")
                .HasColumnName("muscle_group");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.WikiPageUrl)
                .HasColumnType("varchar(255)")
                .HasColumnName("wiki_page_url");
        });

        modelBuilder.Entity<TrainingDay>(entity =>
        {
            entity.ToTable("training_day");

            entity.HasIndex(e => e.Id, "idx_training_day");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingWeekId).HasColumnName("training_week_id");

            entity.HasOne(d => d.TrainingWeek).WithMany(p => p.TrainingDays)
                .HasForeignKey(d => d.TrainingWeekId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Muscles).WithMany(p => p.TrainingDays)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainingDayMuscle",
                    r => r.HasOne<Muscle>().WithMany()
                        .HasForeignKey("MuscleId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<TrainingDay>().WithMany()
                        .HasForeignKey("TrainingDayId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("TrainingDayId", "MuscleId");
                        j.ToTable("training_day_muscle");
                        j.IndexerProperty<int>("TrainingDayId").HasColumnName("training_day_id");
                        j.IndexerProperty<int>("MuscleId").HasColumnName("muscle_id");
                    });
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.ToTable("training_plan");

            entity.HasIndex(e => e.Id, "idx_training_plan_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TrainingDaysPerWeek).HasColumnName("training_days_per_week");
            entity.Property(e => e.TrainingWeeks).HasColumnName("training_weeks");

            entity.HasMany(d => d.Equipment).WithMany(p => p.TrainingPlans)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainingPlanEquipment",
                    r => r.HasOne<Equipment>().WithMany().HasForeignKey("EquipmentId"),
                    l => l.HasOne<TrainingPlan>().WithMany().HasForeignKey("TrainingPlanId"),
                    j =>
                    {
                        j.HasKey("TrainingPlanId", "EquipmentId");
                        j.ToTable("training_plan_equipment");
                        j.IndexerProperty<int>("TrainingPlanId").HasColumnName("training_plan_id");
                        j.IndexerProperty<int>("EquipmentId").HasColumnName("equipment_id");
                    });

            entity.HasMany(d => d.TrainingTypes).WithMany(p => p.TrainingPlans)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainingPlanType",
                    r => r.HasOne<TrainingType>().WithMany().HasForeignKey("TrainingTypeId"),
                    l => l.HasOne<TrainingPlan>().WithMany().HasForeignKey("TrainingPlanId"),
                    j =>
                    {
                        j.HasKey("TrainingPlanId", "TrainingTypeId");
                        j.ToTable("training_plan_type");
                        j.HasIndex(new[] { "TrainingPlanId" }, "idx_training_plan_type_plan_id");
                        j.HasIndex(new[] { "TrainingTypeId" }, "idx_training_plan_type_type_id");
                        j.IndexerProperty<int>("TrainingPlanId").HasColumnName("training_plan_id");
                        j.IndexerProperty<int>("TrainingTypeId").HasColumnName("training_type_id");
                    });
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.ToTable("training_session");

            entity.HasIndex(e => e.CreatedAt, "idx_training_session_created_at");

            entity.HasIndex(e => e.Id, "idx_training_session_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationInSeconds).HasColumnName("duration_in_seconds");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasMany(d => d.TrainingTypes).WithMany(p => p.TrainingSessions)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainingSessionType",
                    r => r.HasOne<TrainingType>().WithMany().HasForeignKey("TrainingTypeId"),
                    l => l.HasOne<TrainingSession>().WithMany().HasForeignKey("TrainingSessionId"),
                    j =>
                    {
                        j.HasKey("TrainingSessionId", "TrainingTypeId");
                        j.ToTable("training_session_type");
                        j.HasIndex(new[] { "TrainingTypeId" }, "idx_training_session_type_training_id");
                        j.HasIndex(new[] { "TrainingSessionId" }, "idx_training_session_type_training_session_id");
                        j.IndexerProperty<int>("TrainingSessionId").HasColumnName("training_session_id");
                        j.IndexerProperty<int>("TrainingTypeId").HasColumnName("training_type_id");
                    });
        });

        modelBuilder.Entity<TrainingSessionExerciseRecord>(entity =>
        {
            entity.ToTable("training_session_exercise_record");

            entity.HasIndex(e => e.CreatedAt, "idx_training_session_exercise_record_created_at");

            entity.HasIndex(e => e.ExerciseRecordId, "idx_training_session_exercise_record_exercise_record_id");

            entity.HasIndex(e => e.TrainingSessionId, "idx_training_session_exercise_record_training_session_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExerciseRecordId).HasColumnName("exercise_record_id");
            entity.Property(e => e.LastWeightUsedKg).HasColumnName("last_weight_used_kg");
            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");

            entity.HasOne(d => d.ExerciseRecord).WithMany(p => p.TrainingSessionExerciseRecords)
                .HasForeignKey(d => d.ExerciseRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.TrainingSessionExerciseRecords)
                .HasForeignKey(d => d.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingType>(entity =>
        {
            entity.ToTable("training_type");

            entity.HasIndex(e => e.Name, "IX_training_type_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<TrainingWeek>(entity =>
        {
            entity.ToTable("training_week");

            entity.HasIndex(e => e.Id, "idx_training_week_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.TrainingWeeksNavigation)
                .HasForeignKey(d => d.TrainingPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });


    }
}
