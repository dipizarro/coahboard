using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoachId",
                table: "Exercises",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "Exercises",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Exercises SET IsGlobal = 1 WHERE CoachId IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CoachId",
                table: "Exercises",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_IsGlobal_CoachId",
                table: "Exercises",
                columns: new[] { "IsGlobal", "CoachId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Coaches_CoachId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CoachId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_IsGlobal_CoachId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "Exercises");
        }
    }
}
