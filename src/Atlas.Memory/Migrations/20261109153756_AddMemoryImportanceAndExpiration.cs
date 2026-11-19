using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Memory.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryImportanceAndExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "Memories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                table: "Memories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "Importance",
                table: "Memories");
        }
    }
}
