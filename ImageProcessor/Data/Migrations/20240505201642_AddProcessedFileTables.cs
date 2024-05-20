using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ImageProcessor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedFileTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedFileStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedFileStatus", x => x.Id);
                }
            );

            migrationBuilder.InsertData(
                table: "ProcessedFileStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Success" },
                    { 2, "Failed" },
                    { 3, "FailedUnsupportedFormat" },
                    { 4, "FailedUnknownFormat" }
                }
            );

            //
            migrationBuilder.CreateTable(
                name: "ProcessedFile",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessedFile_ProcessedFileStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ProcessedFileStatus",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedFile_StatusId",
                table: "ProcessedFile",
                column: "StatusId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProcessedFile");

            migrationBuilder.DropTable(name: "ProcessedFileStatus");
        }
    }
}
