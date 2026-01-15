using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScheduleUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_Name",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_Time",
                table: "Schedules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Name",
                table: "Schedules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_Time",
                table: "Schedules",
                column: "Time",
                unique: true);
        }
    }
}
