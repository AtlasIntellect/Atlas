using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Memory.Migrations
{
    /// <inheritdoc />
    public partial class AddMemoryConfidenceAndProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "Memories",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvenanceData",
                table: "Memories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "ProvenanceData",
                table: "Memories");
        }
    }
}
