using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    weight_kg = table.Column<double>(type: "REAL", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exercise",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    how_to = table.Column<string>(type: "TEXT", nullable: true),
                    difficulty = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "muscle",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    muscle_group = table.Column<string>(type: "varchar(64)", nullable: false),
                    function = table.Column<string>(type: "TEXT", nullable: true),
                    wiki_page_url = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscle", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_plan",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: true),
                    training_weeks = table.Column<int>(type: "INTEGER", nullable: false),
                    training_days_per_week = table.Column<int>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_plan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_session",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    duration_in_seconds = table.Column<int>(type: "INTEGER", nullable: true),
                    calories = table.Column<int>(type: "INTEGER", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    mood = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_type", x => x.id);
                    table.UniqueConstraint("ak_training_type_name", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "exercise_how_to",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    url = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_how_to", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_how_to_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "exercise_record",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: true),
                    repetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    timer_in_seconds = table.Column<int>(type: "INTEGER", nullable: true),
                    distance_in_meters = table.Column<int>(type: "INTEGER", nullable: true),
                    weight_used_kg = table.Column<double>(type: "REAL", nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_record_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "exercise_muscle",
                columns: table => new
                {
                    muscle_id = table.Column<int>(type: "INTEGER", nullable: false),
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_muscle", x => new { x.muscle_id, x.exercise_id });
                    table.ForeignKey(
                        name: "FK_exercise_muscle_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_exercise_muscle_muscle_muscle_id",
                        column: x => x.muscle_id,
                        principalTable: "muscle",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_plan_equipment",
                columns: table => new
                {
                    training_plan_id = table.Column<int>(type: "INTEGER", nullable: false),
                    equipment_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_plan_equipment", x => new { x.training_plan_id, x.equipment_id });
                    table.ForeignKey(
                        name: "FK_training_plan_equipment_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_training_plan_equipment_training_plan_training_plan_id",
                        column: x => x.training_plan_id,
                        principalTable: "training_plan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_week",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    order_number = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    training_plan_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_week", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_week_training_plan_training_plan_id",
                        column: x => x.training_plan_id,
                        principalTable: "training_plan",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "exercise_type",
                columns: table => new
                {
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: false),
                    training_type_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_type", x => new { x.exercise_id, x.training_type_id });
                    table.ForeignKey(
                        name: "FK_exercise_type_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_exercise_type_training_type_training_type_id",
                        column: x => x.training_type_id,
                        principalTable: "training_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_plan_type",
                columns: table => new
                {
                    training_plan_id = table.Column<int>(type: "INTEGER", nullable: false),
                    training_type_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_plan_type", x => new { x.training_plan_id, x.training_type_id });
                    table.ForeignKey(
                        name: "FK_training_plan_type_training_plan_training_plan_id",
                        column: x => x.training_plan_id,
                        principalTable: "training_plan",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_training_plan_type_training_type_training_type_id",
                        column: x => x.training_type_id,
                        principalTable: "training_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_session_type",
                columns: table => new
                {
                    training_session_id = table.Column<int>(type: "INTEGER", nullable: false),
                    training_type_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session_type", x => new { x.training_session_id, x.training_type_id });
                    table.ForeignKey(
                        name: "FK_training_session_type_training_session_training_session_id",
                        column: x => x.training_session_id,
                        principalTable: "training_session",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_training_session_type_training_type_training_type_id",
                        column: x => x.training_type_id,
                        principalTable: "training_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_session_exercise_record",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    training_session_id = table.Column<int>(type: "INTEGER", nullable: true),
                    exercise_record_id = table.Column<int>(type: "INTEGER", nullable: true),
                    last_weight_used_kg = table.Column<double>(type: "REAL", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_session_exercise_record", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_session_exercise_record_exercise_record_exercise_record_id",
                        column: x => x.exercise_record_id,
                        principalTable: "exercise_record",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_training_session_exercise_record_training_session_training_session_id",
                        column: x => x.training_session_id,
                        principalTable: "training_session",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_day",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    order_number = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    training_week_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_day", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_day_training_week_training_week_id",
                        column: x => x.training_week_id,
                        principalTable: "training_week",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "block",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    sets = table.Column<int>(type: "INTEGER", nullable: true),
                    rest_in_seconds = table.Column<int>(type: "INTEGER", nullable: true),
                    instrcustions = table.Column<string>(type: "TEXT", nullable: true),
                    order_number = table.Column<int>(type: "INTEGER", nullable: true),
                    training_day_id = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block", x => x.id);
                    table.ForeignKey(
                        name: "FK_block_training_day_training_day_id",
                        column: x => x.training_day_id,
                        principalTable: "training_day",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "training_day_muscle",
                columns: table => new
                {
                    training_day_id = table.Column<int>(type: "INTEGER", nullable: false),
                    muscle_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_day_muscle", x => new { x.training_day_id, x.muscle_id });
                    table.ForeignKey(
                        name: "FK_training_day_muscle_muscle_muscle_id",
                        column: x => x.muscle_id,
                        principalTable: "muscle",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_training_day_muscle_training_day_training_day_id",
                        column: x => x.training_day_id,
                        principalTable: "training_day",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "block_exercises",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    block_id = table.Column<int>(type: "INTEGER", nullable: true),
                    exercise_id = table.Column<int>(type: "INTEGER", nullable: true),
                    order_number = table.Column<int>(type: "INTEGER", nullable: true),
                    instructions = table.Column<string>(type: "TEXT", nullable: true),
                    repetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    timer_in_seconds = table.Column<int>(type: "INTEGER", nullable: true),
                    distance_in_meters = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_block_exercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_block_exercises_block_block_id",
                        column: x => x.block_id,
                        principalTable: "block",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_block_exercises_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_block_id",
                table: "block",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_block_training_day_id",
                table: "block",
                column: "training_day_id");

            migrationBuilder.CreateIndex(
                name: "IX_block_exercises_block_id",
                table: "block_exercises",
                column: "block_id");

            migrationBuilder.CreateIndex(
                name: "IX_block_exercises_exercise_id",
                table: "block_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "idx_equipment_id",
                table: "equipment",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_difficulty",
                table: "exercise",
                column: "difficulty");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_id",
                table: "exercise",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_name",
                table: "exercise",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_name",
                table: "exercise",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_exercise_how_to_exercise_id",
                table: "exercise_how_to",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_muscle_is_primary",
                table: "exercise_muscle",
                column: "is_primary");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_muscle_exercise_id",
                table: "exercise_muscle",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_record_created_at",
                table: "exercise_record",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_record_id",
                table: "exercise_record",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_record_exercise_id",
                table: "exercise_record",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_type_exercise_id",
                table: "exercise_type",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "idx_exercise_type_training_id",
                table: "exercise_type",
                column: "training_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_muscle_group",
                table: "muscle",
                column: "muscle_group");

            migrationBuilder.CreateIndex(
                name: "idx_muscle_id",
                table: "muscle",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_muscle_name",
                table: "muscle",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_training_day",
                table: "training_day",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_training_day_training_week_id",
                table: "training_day",
                column: "training_week_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_day_muscle_muscle_id",
                table: "training_day_muscle",
                column: "muscle_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_plan_id",
                table: "training_plan",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_training_plan_equipment_equipment_id",
                table: "training_plan_equipment",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_plan_type_plan_id",
                table: "training_plan_type",
                column: "training_plan_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_plan_type_type_id",
                table: "training_plan_type",
                column: "training_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_created_at",
                table: "training_session",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_id",
                table: "training_session",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_exercise_record_created_at",
                table: "training_session_exercise_record",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_exercise_record_exercise_record_id",
                table: "training_session_exercise_record",
                column: "exercise_record_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_exercise_record_training_session_id",
                table: "training_session_exercise_record",
                column: "training_session_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_type_training_id",
                table: "training_session_type",
                column: "training_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_session_type_training_session_id",
                table: "training_session_type",
                column: "training_session_id");

            migrationBuilder.CreateIndex(
                name: "idx_training_type_id",
                table: "training_type",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_training_type_name",
                table: "training_type",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_training_week_id",
                table: "training_week",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_training_week_training_plan_id",
                table: "training_week",
                column: "training_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_exercises");

            migrationBuilder.DropTable(
                name: "exercise_how_to");

            migrationBuilder.DropTable(
                name: "exercise_muscle");

            migrationBuilder.DropTable(
                name: "exercise_type");

            migrationBuilder.DropTable(
                name: "training_day_muscle");

            migrationBuilder.DropTable(
                name: "training_plan_equipment");

            migrationBuilder.DropTable(
                name: "training_plan_type");

            migrationBuilder.DropTable(
                name: "training_session_exercise_record");

            migrationBuilder.DropTable(
                name: "training_session_type");

            migrationBuilder.DropTable(
                name: "block");

            migrationBuilder.DropTable(
                name: "muscle");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "exercise_record");

            migrationBuilder.DropTable(
                name: "training_session");

            migrationBuilder.DropTable(
                name: "training_type");

            migrationBuilder.DropTable(
                name: "training_day");

            migrationBuilder.DropTable(
                name: "exercise");

            migrationBuilder.DropTable(
                name: "training_week");

            migrationBuilder.DropTable(
                name: "training_plan");
        }
    }
}
