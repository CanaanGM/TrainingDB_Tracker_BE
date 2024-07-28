using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Context;

public class SqliteContext : DbContext
{
    public string DatabaseConnectionString { get; set; } = "Data Source = E:/development/c#/TrainingDB_Integration/training_log_v2.db"; //$"{Directory.GetCurrentDirectory()}/training_log_v2.db";
    public SqliteContext()
    {
    }

    public SqliteContext(DbContextOptions<SqliteContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(DatabaseConnectionString);
        }

    }
    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<BlockExercise> BlockExercises { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ExerciseHowTo> ExerciseHowTos { get; set; }

    public virtual DbSet<ExerciseMuscle> ExerciseMuscles { get; set; }

    public virtual DbSet<ExerciseRecord> ExerciseRecords { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<LocalizedEquipment> LocalizedEquipments { get; set; }

    public virtual DbSet<LocalizedExercise> LocalizedExercises { get; set; }

    public virtual DbSet<LocalizedMuscle> LocalizedMuscles { get; set; }

    public virtual DbSet<LocalizedMuscleGroup> LocalizedMuscleGroups { get; set; }

    public virtual DbSet<Measurement> Measurements { get; set; }

    public virtual DbSet<Muscle> Muscles { get; set; }

    public virtual DbSet<MuscleGroup> MuscleGroups { get; set; }

    public virtual DbSet<TrainingDay> TrainingDays { get; set; }

    public virtual DbSet<TrainingPlan> TrainingPlans { get; set; }

    public virtual DbSet<TrainingSession> TrainingSessions { get; set; }

    public virtual DbSet<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; }

    public virtual DbSet<TrainingType> TrainingTypes { get; set; }

    public virtual DbSet<TrainingWeek> TrainingWeeks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserExercise> UserExercises { get; set; }

    public virtual DbSet<UserMuscle> UserMuscles { get; set; }

    public virtual DbSet<UserPassword> UserPasswords { get; set; }

    public virtual DbSet<UserProfileImage> UserProfileImages { get; set; }

    public virtual DbSet<UserTrainingPlan> UserTrainingPlans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Block>(entity =>
        {
            entity.ToTable("block");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.Name).HasColumnName("name");
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

            entity.HasIndex(e => e.WeightKg, "idx_localized_equipment_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercise");

            entity.HasIndex(e => e.Difficulty, "idx_exercise_difficulty");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Difficulty)
                .HasDefaultValue(0)
                .HasColumnName("difficulty");

            entity.HasMany(d => d.Equipment).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseEquipment",
                    r => r.HasOne<Equipment>().WithMany().HasForeignKey("EquipmentId"),
                    l => l.HasOne<Exercise>().WithMany().HasForeignKey("ExerciseId"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "EquipmentId");
                        j.ToTable("exercise_equipment");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("exercise_id");
                        j.IndexerProperty<int>("EquipmentId").HasColumnName("equipment_id");
                    });

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

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Url).HasColumnName("url");

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

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DistanceInMeters).HasColumnName("distance_in_meters");
            entity.Property(e => e.HeartRateAvg).HasColumnName("heart_rate_avg");
            entity.Property(e => e.Incline).HasColumnName("incline");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RateOfPerceivedExertion).HasColumnName("rate_of_perceived_exertion");
            entity.Property(e => e.Repetitions).HasColumnName("repetitions");
            entity.Property(e => e.RestInSeconds).HasColumnName("rest_in_seconds");
            entity.Property(e => e.Speed).HasColumnName("speed");
            entity.Property(e => e.TimerInSeconds).HasColumnName("timer_in_seconds");
            entity.Property(e => e.UserExerciseId).HasColumnName("user_exercise_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeightUsedKg).HasColumnName("weight_used_kg");

            entity.HasOne(d => d.UserExercise).WithMany(p => p.ExerciseRecords)
                .HasForeignKey(d => d.UserExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany(p => p.ExerciseRecords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("language");

            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<LocalizedEquipment>(entity =>
        {
            entity.HasKey(e => new { e.ExerciseId, e.LanguageId });

            entity.ToTable("localized_equipment");

            entity.HasIndex(e => e.Name, "idx_equipment_name");

            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HowTo).HasColumnName("how_to");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Exercise).WithMany(p => p.LocalizedEquipments).HasForeignKey(d => d.ExerciseId);

            entity.HasOne(d => d.Language).WithMany(p => p.LocalizedEquipments).HasForeignKey(d => d.LanguageId);
        });

        modelBuilder.Entity<LocalizedExercise>(entity =>
        {
            entity.HasKey(e => new { e.ExerciseId, e.LanguageId });

            entity.ToTable("localized_exercise");

            entity.HasIndex(e => e.Name, "idx_localized_exercise_name");

            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HowTo).HasColumnName("how_to");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Exercise).WithMany(p => p.LocalizedExercises).HasForeignKey(d => d.ExerciseId);

            entity.HasOne(d => d.Language).WithMany(p => p.LocalizedExercises).HasForeignKey(d => d.LanguageId);
        });

        modelBuilder.Entity<LocalizedMuscle>(entity =>
        {
            entity.HasKey(e => new { e.MuscleId, e.LanguageId });

            entity.ToTable("localized_muscle");

            entity.HasIndex(e => e.Name, "idx_muscle_name");

            entity.Property(e => e.MuscleId).HasColumnName("muscle_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Function).HasColumnName("function");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.WikiPageUrl)
                .HasColumnType("VARCHAR")
                .HasColumnName("wiki_page_url");

            entity.HasOne(d => d.Language).WithMany(p => p.LocalizedMuscles).HasForeignKey(d => d.LanguageId);

            entity.HasOne(d => d.Muscle).WithMany(p => p.LocalizedMuscles).HasForeignKey(d => d.MuscleId);
        });

        modelBuilder.Entity<LocalizedMuscleGroup>(entity =>
        {
            entity.HasKey(e => new { e.MuscleGroupId, e.LanguageId });

            entity.ToTable("localized_muscle_group");

            entity.HasIndex(e => e.Name, "idx_localized_muscle_group_name");

            entity.Property(e => e.MuscleGroupId).HasColumnName("muscle_group_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Function).HasColumnName("function");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.WikiPageUrl).HasColumnName("wiki_page_url");

            entity.HasOne(d => d.Language).WithMany(p => p.LocalizedMuscleGroups).HasForeignKey(d => d.LanguageId);

            entity.HasOne(d => d.MuscleGroup).WithMany(p => p.LocalizedMuscleGroups).HasForeignKey(d => d.MuscleGroupId);
        });

        modelBuilder.Entity<Measurement>(entity =>
        {
            entity.ToTable("measurements");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Chest).HasColumnName("chest");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Hip).HasColumnName("hip");
            entity.Property(e => e.LeftCalf).HasColumnName("left_calf");
            entity.Property(e => e.LeftLowerArm).HasColumnName("left_lower_arm");
            entity.Property(e => e.LeftThigh).HasColumnName("left_thigh");
            entity.Property(e => e.LeftUpperArm).HasColumnName("left_upper_arm");
            entity.Property(e => e.Neck).HasColumnName("neck");
            entity.Property(e => e.RightCalf).HasColumnName("right_calf");
            entity.Property(e => e.RightLowerArm).HasColumnName("right_lower_arm");
            entity.Property(e => e.RightThigh).HasColumnName("right_thigh");
            entity.Property(e => e.RightUpperArm).HasColumnName("right_upper_arm");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Waist).HasColumnName("waist");

            entity.HasOne(d => d.User).WithMany(p => p.Measurements)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.ToTable("muscle");

            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<MuscleGroup>(entity =>
        {
            entity.ToTable("muscle_group");

            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<TrainingDay>(entity =>
        {
            entity.ToTable("training_day");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingWeekId).HasColumnName("training_week_id");

            entity.HasOne(d => d.TrainingWeek).WithMany(p => p.TrainingDays)
                .HasForeignKey(d => d.TrainingWeekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.ToTable("training_plan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.ToTable("training_session");

            entity.HasIndex(e => e.CreatedAt, "idx_training_session_created_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationInSeconds).HasColumnName("duration_in_seconds");
            entity.Property(e => e.Feeling).HasColumnName("feeling");
            entity.Property(e => e.Mood).HasColumnName("mood");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RateOfPerceivedExertionAvg).HasColumnName("rate_of_perceived_exertion_avg");
            entity.Property(e => e.TotalCaloriesBurned).HasColumnName("total_calories_burned");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TrainingSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingSessionExerciseRecord>(entity =>
        {
            entity.HasKey(e => new { e.TrainingSessionId, e.ExerciseRecordId });

            entity.ToTable("training_session_exercise_record");

            entity.HasIndex(e => e.CreatedAt, "idx_training_session_exercise_record_created_at");

            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");
            entity.Property(e => e.ExerciseRecordId).HasColumnName("exercise_record_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LastWeightUsedKg).HasColumnName("last_weight_used_kg");

            entity.HasOne(d => d.ExerciseRecord).WithMany(p => p.TrainingSessionExerciseRecords).HasForeignKey(d => d.ExerciseRecordId);

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.TrainingSessionExerciseRecords).HasForeignKey(d => d.TrainingSessionId);
        });

        modelBuilder.Entity<TrainingType>(entity =>
        {
            entity.ToTable("training_type");

            entity.HasIndex(e => e.Name, "IX_training_type_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Language).WithMany(p => p.TrainingTypes)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TrainingWeek>(entity =>
        {
            entity.ToTable("training_week");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.TrainingWeeks)
                .HasForeignKey(d => d.TrainingPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "IX_user_email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_user_username").IsUnique();

            entity.HasIndex(e => e.Email, "idx_user_email");

            entity.HasIndex(e => e.Username, "idx_user_username");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasColumnType("CHAR(1)")
                .HasColumnName("gender");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<UserExercise>(entity =>
        {
            entity.ToTable("user_exercise");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AverageWeight).HasColumnName("average_weight");
            entity.Property(e => e.BestWeight).HasColumnName("best_weight");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.LoggCount).HasColumnName("logg_count");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Exercise).WithMany(p => p.UserExercises)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany(p => p.UserExercises)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMuscle>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("user_muscle");

            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.MuscleCooldown).HasColumnName("muscle_cooldown");
            entity.Property(e => e.MuscleGroupId).HasColumnName("muscle_group_id");
            entity.Property(e => e.TrainingVolume).HasColumnName("training_volume");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.MuscleGroup).WithMany().HasForeignKey(d => d.MuscleGroupId);

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserPassword>(entity =>
        {
            entity.ToTable("user_passwords");

            entity.HasIndex(e => e.IsCurrent, "idx_user_passwords_is_current");

            entity.HasIndex(e => e.UserId, "idx_user_passwords_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCurrent)
                .HasDefaultValue(true)
                .HasColumnType("BIT")
                .HasColumnName("is_current");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserPasswords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserProfileImage>(entity =>
        {
            entity.ToTable("user_profile_images");

            entity.HasIndex(e => e.UserId, "idx_user_profile_images_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPrimary)
                .HasColumnType("BIT")
                .HasColumnName("is_primary");
            entity.Property(e => e.Url)
                .HasColumnType("varchar(255)")
                .HasColumnName("url");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserProfileImages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTrainingPlan>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.TrainingPlanId });

            entity.ToTable("user_training_plan");

            entity.HasIndex(e => e.IsFinished, "idx_user_training_plan_status");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.EnrolledDate)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("enrolled_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnType("BIT")
                .HasColumnName("is_active");
            entity.Property(e => e.IsFinished)
                .HasDefaultValue(false)
                .HasColumnType("BIT")
                .HasColumnName("is_finished");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
        });

    }
}
