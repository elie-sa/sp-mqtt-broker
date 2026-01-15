using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPBackend.Migrations
{
    /// <inheritdoc />
    public partial class UniqueTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Time",
                table: "Schedules",
                column: "Time",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_Time",
                table: "Schedules");
        }
    }
}
