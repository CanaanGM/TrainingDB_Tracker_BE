using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_block_training_day_training_day_id",
                table: "block");

            migrationBuilder.DropForeignKey(
                name: "FK_block_exercises_block_block_id",
                table: "block_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_block_exercises_exercise_exercise_id",
                table: "block_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_how_to_exercise_exercise_id",
                table: "exercise_how_to");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_muscle_exercise_exercise_id",
                table: "exercise_muscle");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_muscle_muscle_muscle_id",
                table: "exercise_muscle");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_record_exercise_exercise_id",
                table: "exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_type_exercise_exercise_id",
                table: "exercise_type");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_type_training_type_training_type_id",
                table: "exercise_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_day_training_week_training_week_id",
                table: "training_day");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_equipment_equipment_equipment_id",
                table: "training_plan_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_equipment_training_plan_training_plan_id",
                table: "training_plan_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_type_training_plan_training_plan_id",
                table: "training_plan_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_type_training_type_training_type_id",
                table: "training_plan_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_exercise_record_exercise_record_exercise_record_id",
                table: "training_session_exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_exercise_record_training_session_training_session_id",
                table: "training_session_exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_type_training_session_training_session_id",
                table: "training_session_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_type_training_type_training_type_id",
                table: "training_session_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_week_training_plan_training_plan_id",
                table: "training_week");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_training_type_name",
                table: "training_type");

            migrationBuilder.DropIndex(
                name: "idx_training_type_id",
                table: "training_type");

            migrationBuilder.DropIndex(
                name: "idx_training_type_name",
                table: "training_type");

            migrationBuilder.DropIndex(
                name: "idx_muscle_group",
                table: "muscle");

            migrationBuilder.DropIndex(
                name: "idx_muscle_id",
                table: "muscle");

            migrationBuilder.DropIndex(
                name: "idx_muscle_name",
                table: "muscle");

            migrationBuilder.DropIndex(
                name: "idx_exercise_type_exercise_id",
                table: "exercise_type");

            migrationBuilder.DropIndex(
                name: "idx_exercise_difficulty",
                table: "exercise");

            migrationBuilder.DropIndex(
                name: "idx_exercise_id",
                table: "exercise");

            migrationBuilder.DropIndex(
                name: "idx_exercise_name",
                table: "exercise");

            migrationBuilder.RenameIndex(
                name: "idx_exercise_type_training_id",
                table: "exercise_type",
                newName: "IX_exercise_type_training_type_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_primary",
                table: "exercise_muscle",
                type: "boolean",
                nullable: true,
                defaultValueSql: "false",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_type_name",
                table: "training_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_muscle_name",
                table: "muscle",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_block_training_day_training_day_id",
                table: "block",
                column: "training_day_id",
                principalTable: "training_day",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_block_exercises_block_block_id",
                table: "block_exercises",
                column: "block_id",
                principalTable: "block",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_block_exercises_exercise_exercise_id",
                table: "block_exercises",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_how_to_exercise_exercise_id",
                table: "exercise_how_to",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_muscle_exercise_exercise_id",
                table: "exercise_muscle",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_muscle_muscle_muscle_id",
                table: "exercise_muscle",
                column: "muscle_id",
                principalTable: "muscle",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_record_exercise_exercise_id",
                table: "exercise_record",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_type_exercise_exercise_id",
                table: "exercise_type",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_type_training_type_training_type_id",
                table: "exercise_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_day_training_week_training_week_id",
                table: "training_day",
                column: "training_week_id",
                principalTable: "training_week",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_equipment_equipment_equipment_id",
                table: "training_plan_equipment",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_equipment_training_plan_training_plan_id",
                table: "training_plan_equipment",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_type_training_plan_training_plan_id",
                table: "training_plan_type",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_type_training_type_training_type_id",
                table: "training_plan_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_exercise_record_exercise_record_exercise_record_id",
                table: "training_session_exercise_record",
                column: "exercise_record_id",
                principalTable: "exercise_record",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_exercise_record_training_session_training_session_id",
                table: "training_session_exercise_record",
                column: "training_session_id",
                principalTable: "training_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_type_training_session_training_session_id",
                table: "training_session_type",
                column: "training_session_id",
                principalTable: "training_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_type_training_type_training_type_id",
                table: "training_session_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_training_week_training_plan_training_plan_id",
                table: "training_week",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_block_training_day_training_day_id",
                table: "block");

            migrationBuilder.DropForeignKey(
                name: "FK_block_exercises_block_block_id",
                table: "block_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_block_exercises_exercise_exercise_id",
                table: "block_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_how_to_exercise_exercise_id",
                table: "exercise_how_to");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_muscle_exercise_exercise_id",
                table: "exercise_muscle");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_muscle_muscle_muscle_id",
                table: "exercise_muscle");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_record_exercise_exercise_id",
                table: "exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_type_exercise_exercise_id",
                table: "exercise_type");

            migrationBuilder.DropForeignKey(
                name: "FK_exercise_type_training_type_training_type_id",
                table: "exercise_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_day_training_week_training_week_id",
                table: "training_day");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_equipment_equipment_equipment_id",
                table: "training_plan_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_equipment_training_plan_training_plan_id",
                table: "training_plan_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_type_training_plan_training_plan_id",
                table: "training_plan_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_plan_type_training_type_training_type_id",
                table: "training_plan_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_exercise_record_exercise_record_exercise_record_id",
                table: "training_session_exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_exercise_record_training_session_training_session_id",
                table: "training_session_exercise_record");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_type_training_session_training_session_id",
                table: "training_session_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_session_type_training_type_training_type_id",
                table: "training_session_type");

            migrationBuilder.DropForeignKey(
                name: "FK_training_week_training_plan_training_plan_id",
                table: "training_week");

            migrationBuilder.DropIndex(
                name: "IX_training_type_name",
                table: "training_type");

            migrationBuilder.DropIndex(
                name: "IX_muscle_name",
                table: "muscle");

            migrationBuilder.RenameIndex(
                name: "IX_exercise_type_training_type_id",
                table: "exercise_type",
                newName: "idx_exercise_type_training_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_primary",
                table: "exercise_muscle",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldDefaultValueSql: "false");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_training_type_name",
                table: "training_type",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_training_type_id",
                table: "training_type",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "idx_training_type_name",
                table: "training_type",
                column: "name");

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
                name: "idx_exercise_type_exercise_id",
                table: "exercise_type",
                column: "exercise_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_block_training_day_training_day_id",
                table: "block",
                column: "training_day_id",
                principalTable: "training_day",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_block_exercises_block_block_id",
                table: "block_exercises",
                column: "block_id",
                principalTable: "block",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_block_exercises_exercise_exercise_id",
                table: "block_exercises",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_how_to_exercise_exercise_id",
                table: "exercise_how_to",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_muscle_exercise_exercise_id",
                table: "exercise_muscle",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_muscle_muscle_muscle_id",
                table: "exercise_muscle",
                column: "muscle_id",
                principalTable: "muscle",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_record_exercise_exercise_id",
                table: "exercise_record",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_type_exercise_exercise_id",
                table: "exercise_type",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercise_type_training_type_training_type_id",
                table: "exercise_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_day_training_week_training_week_id",
                table: "training_day",
                column: "training_week_id",
                principalTable: "training_week",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_equipment_equipment_equipment_id",
                table: "training_plan_equipment",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_equipment_training_plan_training_plan_id",
                table: "training_plan_equipment",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_type_training_plan_training_plan_id",
                table: "training_plan_type",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_plan_type_training_type_training_type_id",
                table: "training_plan_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_exercise_record_exercise_record_exercise_record_id",
                table: "training_session_exercise_record",
                column: "exercise_record_id",
                principalTable: "exercise_record",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_exercise_record_training_session_training_session_id",
                table: "training_session_exercise_record",
                column: "training_session_id",
                principalTable: "training_session",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_type_training_session_training_session_id",
                table: "training_session_type",
                column: "training_session_id",
                principalTable: "training_session",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_session_type_training_type_training_type_id",
                table: "training_session_type",
                column: "training_type_id",
                principalTable: "training_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_training_week_training_plan_training_plan_id",
                table: "training_week",
                column: "training_plan_id",
                principalTable: "training_plan",
                principalColumn: "id");
        }
    }
}
