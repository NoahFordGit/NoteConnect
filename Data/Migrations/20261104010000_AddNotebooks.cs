using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteConnect.Data.Migrations
{
    public partial class AddNotebooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notebooks",
                columns: table => new
                {
                    NotebookId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotebookName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notebooks", x => x.NotebookId);
                });

            // Earlier Note forms allowed an arbitrary NotebookId without a parent row.
            // Move those legacy notes to Unfiled before adding referential integrity.
            migrationBuilder.Sql("UPDATE \"Notes\" SET \"NotebookId\" = NULL WHERE \"NotebookId\" IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NotebookId",
                table: "Notes",
                column: "NotebookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Notebooks_NotebookId",
                table: "Notes",
                column: "NotebookId",
                principalTable: "Notebooks",
                principalColumn: "NotebookId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Notebooks_NotebookId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_NotebookId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "Notebooks");
        }
    }
}
