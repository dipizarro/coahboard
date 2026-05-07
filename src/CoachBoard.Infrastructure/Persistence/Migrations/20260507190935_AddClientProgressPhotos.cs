using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientProgressPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientProgressPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ClientProgressRecordId = table.Column<int>(type: "int", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PhotoType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProgressPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProgressPhotos_ClientProgressRecords_ClientProgressRecordId",
                        column: x => x.ClientProgressRecordId,
                        principalTable: "ClientProgressRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientProgressPhotos_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProgressPhotos_ClientId_TakenAt",
                table: "ClientProgressPhotos",
                columns: new[] { "ClientId", "TakenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProgressPhotos_ClientProgressRecordId",
                table: "ClientProgressPhotos",
                column: "ClientProgressRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientProgressPhotos");
        }
    }
}
