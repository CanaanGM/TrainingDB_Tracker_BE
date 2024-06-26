using System;
using System.Collections.Generic;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Context;

public partial class TrainingLogDbContext : DbContext
{
    public TrainingLogDbContext()
    {
    }

    public TrainingLogDbContext(DbContextOptions<TrainingLogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<AuthorLink> AuthorLinks { get; set; }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<BlockExercise> BlockExercises { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<EquipmentMuscle> EquipmentMuscles { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ExerciseHowTo> ExerciseHowTos { get; set; }

    public virtual DbSet<ExerciseMuscle> ExerciseMuscles { get; set; }

    public virtual DbSet<ExerciseRecord> ExerciseRecords { get; set; }

    public virtual DbSet<ExerciseType> ExerciseTypes { get; set; }

    public virtual DbSet<Muscle> Muscles { get; set; }

    public virtual DbSet<MuscleGroup> MuscleGroups { get; set; }

    public virtual DbSet<MuscleGroupMuscle> MuscleGroupMuscles { get; set; }

    public virtual DbSet<TrainingDay> TrainingDays { get; set; }

    public virtual DbSet<TrainingDayBlock> TrainingDayBlocks { get; set; }

    public virtual DbSet<TrainingDayMuscleGroup> TrainingDayMuscleGroups { get; set; }

    public virtual DbSet<TrainingPlan> TrainingPlans { get; set; }

    public virtual DbSet<TrainingPlanEquipment> TrainingPlanEquipments { get; set; }

    public virtual DbSet<TrainingPlanType> TrainingPlanTypes { get; set; }

    public virtual DbSet<TrainingPlanWeek> TrainingPlanWeeks { get; set; }

    public virtual DbSet<TrainingSession> TrainingSessions { get; set; }

    public virtual DbSet<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; }

    public virtual DbSet<TrainingSessionType> TrainingSessionTypes { get; set; }

    public virtual DbSet<TrainingType> TrainingTypes { get; set; }

    public virtual DbSet<TrainingWeek> TrainingWeeks { get; set; }

    public virtual DbSet<TrainingWeekDay> TrainingWeekDays { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserImage> UserImages { get; set; }

    public virtual DbSet<UserPassword> UserPasswords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=E:/development/databases/training_log_db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("author");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<AuthorLink>(entity =>
        {
            entity.ToTable("author_links");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Url)
                .HasColumnType("varchar(255)")
                .HasColumnName("url");
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.ToTable("block");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Instrcustions).HasColumnName("instrcustions");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.RestInSeconds).HasColumnName("rest_in_seconds");
            entity.Property(e => e.Sets).HasColumnName("sets");
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

            entity.HasOne(d => d.Block).WithMany(p => p.BlockExercises).HasForeignKey(d => d.BlockId);

            entity.HasOne(d => d.Exercise).WithMany(p => p.BlockExercises).HasForeignKey(d => d.ExerciseId);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("equipment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Weight).HasColumnName("weight");
        });

        modelBuilder.Entity<EquipmentMuscle>(entity =>
        {
            entity.ToTable("equipment_muscle");

            entity.HasIndex(e => new { e.EquipmentId, e.MuscleId }, "IX_equipment_muscle_equipment_id_muscle_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.IsPrimary)
                .HasColumnType("boolean")
                .HasColumnName("is_primary");
            entity.Property(e => e.MuscleId).HasColumnName("muscle_id");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EquipmentMuscles).HasForeignKey(d => d.EquipmentId);

            entity.HasOne(d => d.Muscle).WithMany(p => p.EquipmentMuscles).HasForeignKey(d => d.MuscleId);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercise");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("text\r\n	how_to text")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<ExerciseHowTo>(entity =>
        {
            entity.ToTable("exercise_how_to");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Url)
                .HasColumnType("varchar(255)")
                .HasColumnName("url");
        });

        modelBuilder.Entity<ExerciseMuscle>(entity =>
        {
            entity.ToTable("exercise_muscle");

            entity.HasIndex(e => new { e.MuscleId, e.ExerciseId }, "IX_exercise_muscle_muscle_id_exercise_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.IsPrimary)
                .HasColumnType("boolean")
                .HasColumnName("is_primary");
            entity.Property(e => e.MuscleId).HasColumnName("muscle_id");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseMuscles).HasForeignKey(d => d.ExerciseId);

            entity.HasOne(d => d.Muscle).WithMany(p => p.ExerciseMuscles).HasForeignKey(d => d.MuscleId);
        });

        modelBuilder.Entity<ExerciseRecord>(entity =>
        {
            entity.ToTable("exercise_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DistanceInMeters).HasColumnName("distance_in_meters");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.LastWeightUsed).HasColumnName("last_weight_used");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Repetitions).HasColumnName("repetitions");
            entity.Property(e => e.TimerInSeconds).HasColumnName("timer_in_seconds");
            entity.Property(e => e.WeightUsed).HasColumnName("weight_used");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseRecords).HasForeignKey(d => d.ExerciseId);
        });

        modelBuilder.Entity<ExerciseType>(entity =>
        {
            entity.ToTable("exercise_type");

            entity.HasIndex(e => new { e.ExerciseId, e.TrainingTypeId }, "IX_exercise_type_exercise_id_training_type_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExerciseId).HasColumnName("exercise_id");
            entity.Property(e => e.TrainingTypeId).HasColumnName("training_type_id");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseTypes).HasForeignKey(d => d.ExerciseId);

            entity.HasOne(d => d.TrainingType).WithMany(p => p.ExerciseTypes).HasForeignKey(d => d.TrainingTypeId);
        });

        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.ToTable("muscle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Function).HasColumnName("function");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.WikiPageUrl)
                .HasColumnType("varchar(255)")
                .HasColumnName("wiki_page_url");
        });

        modelBuilder.Entity<MuscleGroup>(entity =>
        {
            entity.ToTable("muscle_group");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommonName)
                .HasColumnType("varchar(64)")
                .HasColumnName("common_name");
            entity.Property(e => e.Function).HasColumnName("function");
            entity.Property(e => e.ScientificName)
                .HasColumnType("varchar(64)")
                .HasColumnName("scientific_name");
            entity.Property(e => e.WikiPageUrl)
                .HasColumnType("varchar(255)")
                .HasColumnName("wiki_page_url");
        });

        modelBuilder.Entity<MuscleGroupMuscle>(entity =>
        {
            entity.ToTable("muscle_group_muscle");

            entity.HasIndex(e => new { e.MuscleGroupId, e.MuscleId }, "IX_muscle_group_muscle_muscle_group_id_muscle_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MuscleGroupId).HasColumnName("muscle_group_id");
            entity.Property(e => e.MuscleId).HasColumnName("muscle_id");

            entity.HasOne(d => d.MuscleGroup).WithMany(p => p.MuscleGroupMuscles).HasForeignKey(d => d.MuscleGroupId);

            entity.HasOne(d => d.Muscle).WithMany(p => p.MuscleGroupMuscles).HasForeignKey(d => d.MuscleId);
        });

        modelBuilder.Entity<TrainingDay>(entity =>
        {
            entity.ToTable("training_day");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<TrainingDayBlock>(entity =>
        {
            entity.ToTable("training_day_blocks");

            entity.HasIndex(e => new { e.TrainingDayId, e.BlockId }, "IX_training_day_blocks_training_day_id_block_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingDayId).HasColumnName("training_day_id");

            entity.HasOne(d => d.Block).WithMany(p => p.TrainingDayBlocks).HasForeignKey(d => d.BlockId);

            entity.HasOne(d => d.TrainingDay).WithMany(p => p.TrainingDayBlocks).HasForeignKey(d => d.TrainingDayId);
        });

        modelBuilder.Entity<TrainingDayMuscleGroup>(entity =>
        {
            entity.ToTable("training_day_muscle_groups");

            entity.HasIndex(e => new { e.TrainingDayId, e.MuscleGroupId }, "IX_training_day_muscle_groups_training_day_id_muscle_group_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MuscleGroupId).HasColumnName("muscle_group_id");
            entity.Property(e => e.TrainingDayId).HasColumnName("training_day_id");

            entity.HasOne(d => d.MuscleGroup).WithMany(p => p.TrainingDayMuscleGroups).HasForeignKey(d => d.MuscleGroupId);

            entity.HasOne(d => d.TrainingDay).WithMany(p => p.TrainingDayMuscleGroups).HasForeignKey(d => d.TrainingDayId);
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
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TrainingDaysPerWeek).HasColumnName("training_days_per_week");
            entity.Property(e => e.TrainingWeeks).HasColumnName("training_weeks");
        });

        modelBuilder.Entity<TrainingPlanEquipment>(entity =>
        {
            entity.ToTable("training_plan_equipment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");

            entity.HasOne(d => d.Equipment).WithMany(p => p.TrainingPlanEquipments).HasForeignKey(d => d.EquipmentId);

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.TrainingPlanEquipments).HasForeignKey(d => d.TrainingPlanId);
        });

        modelBuilder.Entity<TrainingPlanType>(entity =>
        {
            entity.ToTable("training_plan_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");
            entity.Property(e => e.TrainingTypeId).HasColumnName("training_type_id");

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.TrainingPlanTypes).HasForeignKey(d => d.TrainingPlanId);

            entity.HasOne(d => d.TrainingType).WithMany(p => p.TrainingPlanTypes).HasForeignKey(d => d.TrainingTypeId);
        });

        modelBuilder.Entity<TrainingPlanWeek>(entity =>
        {
            entity.ToTable("training_plan_weeks");

            entity.HasIndex(e => new { e.TrainingPlanId, e.TrainingWeekId }, "IX_training_plan_weeks_training_plan_id_training_week_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.TrainingPlanId).HasColumnName("training_plan_id");
            entity.Property(e => e.TrainingWeekId).HasColumnName("training_week_id");

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.TrainingPlanWeeks).HasForeignKey(d => d.TrainingPlanId);

            entity.HasOne(d => d.TrainingWeek).WithMany(p => p.TrainingPlanWeeks).HasForeignKey(d => d.TrainingWeekId);
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.ToTable("training_session");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Notes).HasColumnName("notes");
        });

        modelBuilder.Entity<TrainingSessionExerciseRecord>(entity =>
        {
            entity.ToTable("training_session_exercise_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExerciseRecordId).HasColumnName("exercise_record_id");
            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");

            entity.HasOne(d => d.ExerciseRecord).WithMany(p => p.TrainingSessionExerciseRecords).HasForeignKey(d => d.ExerciseRecordId);

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.TrainingSessionExerciseRecords).HasForeignKey(d => d.TrainingSessionId);
        });

        modelBuilder.Entity<TrainingSessionType>(entity =>
        {
            entity.ToTable("training_session_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");
            entity.Property(e => e.TrainingTypeId).HasColumnName("training_type_id");

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.TrainingSessionTypes).HasForeignKey(d => d.TrainingSessionId);

            entity.HasOne(d => d.TrainingType).WithMany(p => p.TrainingSessionTypes).HasForeignKey(d => d.TrainingTypeId);
        });

        modelBuilder.Entity<TrainingType>(entity =>
        {
            entity.ToTable("training_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
        });

        modelBuilder.Entity<TrainingWeek>(entity =>
        {
            entity.ToTable("training_week");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Mesocycle).HasColumnName("mesocycle");
            entity.Property(e => e.Name)
                .HasColumnType("varchar(64)")
                .HasColumnName("name");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
        });

        modelBuilder.Entity<TrainingWeekDay>(entity =>
        {
            entity.ToTable("training_week_days");

            entity.HasIndex(e => new { e.TrainingWeekId, e.TrainingDayId }, "IX_training_week_days_training_week_id_training_day_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.TrainingDayId).HasColumnName("training_day_id");
            entity.Property(e => e.TrainingWeekId).HasColumnName("training_week_id");

            entity.HasOne(d => d.TrainingDay).WithMany(p => p.TrainingWeekDays).HasForeignKey(d => d.TrainingDayId);

            entity.HasOne(d => d.TrainingWeek).WithMany(p => p.TrainingWeekDays).HasForeignKey(d => d.TrainingWeekId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "IX_user_email").IsUnique();

            entity.HasIndex(e => e.Username, "IX_user_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasColumnType("varchar(255)")
                .HasColumnName("email");
            entity.Property(e => e.Username)
                .HasColumnType("varchar(255)")
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserImage>(entity =>
        {
            entity.ToTable("user_images");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPrimary)
                .HasColumnType("boolean")
                .HasColumnName("is_primary");
            entity.Property(e => e.Url)
                .HasColumnType("varchar(255)")
                .HasColumnName("url");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserImages).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<UserPassword>(entity =>
        {
            entity.ToTable("user_passwords");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasColumnType("boolean")
                .HasColumnName("is_active");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserPasswords).HasForeignKey(d => d.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
