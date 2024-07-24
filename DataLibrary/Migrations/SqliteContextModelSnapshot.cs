﻿// <auto-generated />
using System;
using DataLibrary.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataLibrary.Migrations
{
    [DbContext(typeof(SqliteContext))]
    partial class SqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("DataLibrary.Models.Block", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Instructions")
                        .HasColumnType("TEXT")
                        .HasColumnName("instructions");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<int?>("OrderNumber")
                        .HasColumnType("INTEGER")
                        .HasColumnName("order_number");

                    b.Property<int?>("RestInSeconds")
                        .HasColumnType("INTEGER")
                        .HasColumnName("rest_in_seconds");

                    b.Property<int?>("Sets")
                        .HasColumnType("INTEGER")
                        .HasColumnName("sets");

                    b.Property<int?>("TrainingDayId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_day_id");

                    b.HasKey("Id");

                    b.HasIndex("TrainingDayId");

                    b.HasIndex(new[] { "Id" }, "idx_block_id");

                    b.ToTable("block", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.BlockExercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int?>("BlockId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("block_id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int?>("DistanceInMeters")
                        .HasColumnType("INTEGER")
                        .HasColumnName("distance_in_meters");

                    b.Property<int?>("ExerciseId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_id");

                    b.Property<string>("Instructions")
                        .HasColumnType("TEXT")
                        .HasColumnName("instructions");

                    b.Property<int?>("OrderNumber")
                        .HasColumnType("INTEGER")
                        .HasColumnName("order_number");

                    b.Property<int?>("Repetitions")
                        .HasColumnType("INTEGER")
                        .HasColumnName("repetitions");

                    b.Property<int?>("TimerInSeconds")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timer_in_seconds");

                    b.HasKey("Id");

                    b.HasIndex("BlockId");

                    b.HasIndex("ExerciseId");

                    b.ToTable("block_exercises", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.Equipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<double?>("WeightKg")
                        .HasColumnType("REAL")
                        .HasColumnName("weight_kg");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "idx_equipment_id");

                    b.ToTable("equipment", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.Exercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<int?>("Difficulty")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0)
                        .HasColumnName("difficulty");

                    b.Property<string>("HowTo")
                        .HasColumnType("TEXT")
                        .HasColumnName("how_to");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "IX_exercise_name")
                        .IsUnique();

                    b.ToTable("exercise", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseHowTo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int?>("ExerciseId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("varchar(255)")
                        .HasColumnName("url");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "ExerciseId" }, "idx_exercise_how_to_exercise_id");

                    b.ToTable("exercise_how_to", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseMuscle", b =>
                {
                    b.Property<int>("MuscleId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("muscle_id");

                    b.Property<int>("ExerciseId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_id");

                    b.Property<bool?>("IsPrimary")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasColumnName("is_primary")
                        .HasDefaultValueSql("false");

                    b.HasKey("MuscleId", "ExerciseId");

                    b.HasIndex("ExerciseId");

                    b.HasIndex(new[] { "IsPrimary" }, "idx_exercise_muscle_is_primary");

                    b.ToTable("exercise_muscle", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int?>("DistanceInMeters")
                        .HasColumnType("INTEGER")
                        .HasColumnName("distance_in_meters");

                    b.Property<int?>("ExerciseId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_id");

                    b.Property<int?>("HeartRateAvg")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Incline")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("KcalBurned")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT")
                        .HasColumnName("notes");

                    b.Property<int?>("RateOfPerceivedExertion")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Repetitions")
                        .HasColumnType("INTEGER")
                        .HasColumnName("repetitions");

                    b.Property<int?>("RestInSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Speed")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TimerInSeconds")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timer_in_seconds");

                    b.Property<double?>("WeightUsedKg")
                        .HasColumnType("REAL")
                        .HasColumnName("weight_used_kg");

                    b.HasKey("Id");

                    b.HasIndex("ExerciseId");

                    b.HasIndex(new[] { "CreatedAt" }, "idx_exercise_record_created_at");

                    b.HasIndex(new[] { "Id" }, "idx_exercise_record_id");

                    b.ToTable("exercise_record", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.Measurements", b =>
                {
                    b.Property<int>("MeasurementsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BasalMetabolicRate")
                        .HasColumnType("INTEGER")
                        .HasColumnName("basal_metabolic_rate");

                    b.Property<double>("BodyFatMass")
                        .HasColumnType("REAL")
                        .HasColumnName("body_fat_mass");

                    b.Property<double>("BodyFatPercent")
                        .HasColumnType("REAL")
                        .HasColumnName("body_fat_percent");

                    b.Property<double>("BodyMassIndex")
                        .HasColumnType("REAL")
                        .HasColumnName("body_mass_index");

                    b.Property<double>("BodyWeight")
                        .HasColumnType("REAL")
                        .HasColumnName("body_weight");

                    b.Property<int>("Chest")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int>("Hip")
                        .HasColumnType("INTEGER")
                        .HasColumnName("hip");

                    b.Property<double>("InBodyScore")
                        .HasColumnType("REAL")
                        .HasColumnName("in_body_score");

                    b.Property<int>("LeftCalf")
                        .HasColumnType("INTEGER")
                        .HasColumnName("left_calf");

                    b.Property<int>("LeftForearm")
                        .HasColumnType("INTEGER")
                        .HasColumnName("left_forearm");

                    b.Property<int>("LeftThigh")
                        .HasColumnType("INTEGER")
                        .HasColumnName("left_thigh");

                    b.Property<int>("LeftUpperArm")
                        .HasColumnType("INTEGER")
                        .HasColumnName("left_upper_arm");

                    b.Property<double>("Minerals")
                        .HasColumnType("REAL")
                        .HasColumnName("minerals");

                    b.Property<int>("Neck")
                        .HasColumnType("INTEGER")
                        .HasColumnName("neck");

                    b.Property<double>("Protein")
                        .HasColumnType("REAL")
                        .HasColumnName("protein");

                    b.Property<int>("RightCalf")
                        .HasColumnType("INTEGER")
                        .HasColumnName("right_calf");

                    b.Property<int>("RightForearm")
                        .HasColumnType("INTEGER")
                        .HasColumnName("right_forearm");

                    b.Property<int>("RightThigh")
                        .HasColumnType("INTEGER")
                        .HasColumnName("right_thigh");

                    b.Property<int>("RightUpperArm")
                        .HasColumnType("INTEGER")
                        .HasColumnName("right_upper_arm");

                    b.Property<double>("SkeletalMuscleMass")
                        .HasColumnType("REAL")
                        .HasColumnName("skeletal_muscle_mass");

                    b.Property<double>("TotalBodyWater")
                        .HasColumnType("REAL")
                        .HasColumnName("total_body_water");

                    b.Property<int>("VisceralFatLevel")
                        .HasColumnType("INTEGER")
                        .HasColumnName("visceral_fat_level");

                    b.Property<int>("WaistOnBelly")
                        .HasColumnType("INTEGER")
                        .HasColumnName("waist_on_belly");

                    b.Property<int>("WaistUnderBelly")
                        .HasColumnType("INTEGER")
                        .HasColumnName("waist_under_belly");

                    b.HasKey("MeasurementsId");

                    b.HasIndex(new[] { "MeasurementsId" }, "idx_measurements_id");

                    b.ToTable("measurements", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.Muscle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Function")
                        .HasColumnType("TEXT")
                        .HasColumnName("function");

                    b.Property<string>("MuscleGroup")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("muscle_group");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<string>("WikiPageUrl")
                        .HasColumnType("varchar(255)")
                        .HasColumnName("wiki_page_url");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "IX_muscle_name")
                        .IsUnique();

                    b.ToTable("muscle", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingDay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT")
                        .HasColumnName("notes");

                    b.Property<int?>("OrderNumber")
                        .HasColumnType("INTEGER")
                        .HasColumnName("order_number");

                    b.Property<int?>("TrainingWeekId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_week_id");

                    b.HasKey("Id");

                    b.HasIndex("TrainingWeekId");

                    b.HasIndex(new[] { "Id" }, "idx_training_day");

                    b.ToTable("training_day", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT")
                        .HasColumnName("notes");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "idx_training_plan_id");

                    b.ToTable("training_plan", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingSession", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int?>("DurationInSeconds")
                        .HasColumnType("INTEGER")
                        .HasColumnName("duration_in_seconds");

                    b.Property<int?>("Mood")
                        .HasColumnType("INTEGER")
                        .HasColumnName("mood");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT")
                        .HasColumnName("notes");

                    b.Property<int?>("TotalCaloriesBurned")
                        .HasColumnType("INTEGER")
                        .HasColumnName("calories");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CreatedAt" }, "idx_training_session_created_at");

                    b.HasIndex(new[] { "Id" }, "idx_training_session_id");

                    b.ToTable("training_session", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingSessionExerciseRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<int?>("ExerciseRecordId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_record_id");

                    b.Property<double?>("LastWeightUsedKg")
                        .HasColumnType("REAL")
                        .HasColumnName("last_weight_used_kg");

                    b.Property<int?>("TrainingSessionId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_session_id");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "CreatedAt" }, "idx_training_session_exercise_record_created_at");

                    b.HasIndex(new[] { "ExerciseRecordId" }, "idx_training_session_exercise_record_exercise_record_id");

                    b.HasIndex(new[] { "TrainingSessionId" }, "idx_training_session_exercise_record_training_session_id");

                    b.ToTable("training_session_exercise_record", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "IX_training_type_name")
                        .IsUnique();

                    b.ToTable("training_type", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingWeek", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("INTEGER")
                        .HasColumnName("order_number");

                    b.Property<int?>("TrainingPlanId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_plan_id");

                    b.HasKey("Id");

                    b.HasIndex("TrainingPlanId");

                    b.HasIndex(new[] { "Id" }, "idx_training_week_id");

                    b.ToTable("training_week", (string)null);
                });

            modelBuilder.Entity("ExerciseType", b =>
                {
                    b.Property<int>("ExerciseId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("exercise_id");

                    b.Property<int>("TrainingTypeId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_type_id");

                    b.HasKey("ExerciseId", "TrainingTypeId");

                    b.HasIndex("TrainingTypeId");

                    b.ToTable("exercise_type", (string)null);
                });

            modelBuilder.Entity("TrainingDayMuscle", b =>
                {
                    b.Property<int>("TrainingDayId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_day_id");

                    b.Property<int>("MuscleId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("muscle_id");

                    b.HasKey("TrainingDayId", "MuscleId");

                    b.HasIndex("MuscleId");

                    b.ToTable("training_day_muscle", (string)null);
                });

            modelBuilder.Entity("TrainingPlanEquipment", b =>
                {
                    b.Property<int>("TrainingPlanId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_plan_id");

                    b.Property<int>("EquipmentId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("equipment_id");

                    b.HasKey("TrainingPlanId", "EquipmentId");

                    b.HasIndex("EquipmentId");

                    b.ToTable("training_plan_equipment", (string)null);
                });

            modelBuilder.Entity("TrainingPlanType", b =>
                {
                    b.Property<int>("TrainingPlanId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_plan_id");

                    b.Property<int>("TrainingTypeId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_type_id");

                    b.HasKey("TrainingPlanId", "TrainingTypeId");

                    b.HasIndex(new[] { "TrainingPlanId" }, "idx_training_plan_type_plan_id");

                    b.HasIndex(new[] { "TrainingTypeId" }, "idx_training_plan_type_type_id");

                    b.ToTable("training_plan_type", (string)null);
                });

            modelBuilder.Entity("TrainingSessionType", b =>
                {
                    b.Property<int>("TrainingSessionId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_session_id");

                    b.Property<int>("TrainingTypeId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("training_type_id");

                    b.HasKey("TrainingSessionId", "TrainingTypeId");

                    b.HasIndex(new[] { "TrainingTypeId" }, "idx_training_session_type_training_id");

                    b.HasIndex(new[] { "TrainingSessionId" }, "idx_training_session_type_training_session_id");

                    b.ToTable("training_session_type", (string)null);
                });

            modelBuilder.Entity("DataLibrary.Models.Block", b =>
                {
                    b.HasOne("DataLibrary.Models.TrainingDay", "TrainingDay")
                        .WithMany("Blocks")
                        .HasForeignKey("TrainingDayId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("TrainingDay");
                });

            modelBuilder.Entity("DataLibrary.Models.BlockExercise", b =>
                {
                    b.HasOne("DataLibrary.Models.Block", "Block")
                        .WithMany("BlockExercises")
                        .HasForeignKey("BlockId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataLibrary.Models.Exercise", "Exercise")
                        .WithMany("BlockExercises")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Block");

                    b.Navigation("Exercise");
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseHowTo", b =>
                {
                    b.HasOne("DataLibrary.Models.Exercise", "Exercise")
                        .WithMany("ExerciseHowTos")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Exercise");
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseMuscle", b =>
                {
                    b.HasOne("DataLibrary.Models.Exercise", "Exercise")
                        .WithMany("ExerciseMuscles")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.Muscle", "Muscle")
                        .WithMany("ExerciseMuscles")
                        .HasForeignKey("MuscleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("Muscle");
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseRecord", b =>
                {
                    b.HasOne("DataLibrary.Models.Exercise", "Exercise")
                        .WithMany("ExerciseRecords")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Exercise");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingDay", b =>
                {
                    b.HasOne("DataLibrary.Models.TrainingWeek", "TrainingWeek")
                        .WithMany("TrainingDays")
                        .HasForeignKey("TrainingWeekId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("TrainingWeek");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingSessionExerciseRecord", b =>
                {
                    b.HasOne("DataLibrary.Models.ExerciseRecord", "ExerciseRecord")
                        .WithMany("TrainingSessionExerciseRecords")
                        .HasForeignKey("ExerciseRecordId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataLibrary.Models.TrainingSession", "TrainingSession")
                        .WithMany("TrainingSessionExerciseRecords")
                        .HasForeignKey("TrainingSessionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ExerciseRecord");

                    b.Navigation("TrainingSession");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingWeek", b =>
                {
                    b.HasOne("DataLibrary.Models.TrainingPlan", "TrainingPlan")
                        .WithMany("TrainingWeeksNavigation")
                        .HasForeignKey("TrainingPlanId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("TrainingPlan");
                });

            modelBuilder.Entity("ExerciseType", b =>
                {
                    b.HasOne("DataLibrary.Models.Exercise", null)
                        .WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.TrainingType", null)
                        .WithMany()
                        .HasForeignKey("TrainingTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TrainingDayMuscle", b =>
                {
                    b.HasOne("DataLibrary.Models.Muscle", null)
                        .WithMany()
                        .HasForeignKey("MuscleId")
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.TrainingDay", null)
                        .WithMany()
                        .HasForeignKey("TrainingDayId")
                        .IsRequired();
                });

            modelBuilder.Entity("TrainingPlanEquipment", b =>
                {
                    b.HasOne("DataLibrary.Models.Equipment", null)
                        .WithMany()
                        .HasForeignKey("EquipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.TrainingPlan", null)
                        .WithMany()
                        .HasForeignKey("TrainingPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TrainingPlanType", b =>
                {
                    b.HasOne("DataLibrary.Models.TrainingPlan", null)
                        .WithMany()
                        .HasForeignKey("TrainingPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.TrainingType", null)
                        .WithMany()
                        .HasForeignKey("TrainingTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TrainingSessionType", b =>
                {
                    b.HasOne("DataLibrary.Models.TrainingSession", null)
                        .WithMany()
                        .HasForeignKey("TrainingSessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLibrary.Models.TrainingType", null)
                        .WithMany()
                        .HasForeignKey("TrainingTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataLibrary.Models.Block", b =>
                {
                    b.Navigation("BlockExercises");
                });

            modelBuilder.Entity("DataLibrary.Models.Exercise", b =>
                {
                    b.Navigation("BlockExercises");

                    b.Navigation("ExerciseHowTos");

                    b.Navigation("ExerciseMuscles");

                    b.Navigation("ExerciseRecords");
                });

            modelBuilder.Entity("DataLibrary.Models.ExerciseRecord", b =>
                {
                    b.Navigation("TrainingSessionExerciseRecords");
                });

            modelBuilder.Entity("DataLibrary.Models.Muscle", b =>
                {
                    b.Navigation("ExerciseMuscles");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingDay", b =>
                {
                    b.Navigation("Blocks");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingPlan", b =>
                {
                    b.Navigation("TrainingWeeksNavigation");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingSession", b =>
                {
                    b.Navigation("TrainingSessionExerciseRecords");
                });

            modelBuilder.Entity("DataLibrary.Models.TrainingWeek", b =>
                {
                    b.Navigation("TrainingDays");
                });
#pragma warning restore 612, 618
        }
    }
}
