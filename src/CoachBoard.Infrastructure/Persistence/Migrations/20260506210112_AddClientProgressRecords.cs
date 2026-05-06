using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProgressRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientProgressRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    BodyFatPercentage = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ChestCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    WaistCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    HipCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    LeftArmCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    RightArmCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    LeftThighCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    RightThighCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    RestingHeartRate = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProgressRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProgressRecords_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProgressRecords_ClientId_RecordedAt",
                table: "ClientProgressRecords",
                columns: new[] { "ClientId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientProgressRecords");
        }
    }
}
