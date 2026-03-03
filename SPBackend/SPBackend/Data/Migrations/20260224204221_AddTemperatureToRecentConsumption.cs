using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTemperatureToRecentConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "RecentConsumptions",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "RecentConsumptions");
        }
    }
}
