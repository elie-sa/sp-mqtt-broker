using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SPBackend.Migrations
{
    /// <inheritdoc />
    public partial class PricePerConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalPrice",
                table: "Consumptions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "PowerSourceSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PowerSourceId = table.Column<long>(type: "bigint", nullable: false),
                    HouseholdId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerSourceSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerSourceSessions_Households_HouseholdId",
                        column: x => x.HouseholdId,
                        principalTable: "Households",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PowerSourceSessions_PowerSources_PowerSourceId",
                        column: x => x.PowerSourceId,
                        principalTable: "PowerSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerSourceSessions_HouseholdId",
                table: "PowerSourceSessions",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerSourceSessions_PowerSourceId",
                table: "PowerSourceSessions",
                column: "PowerSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerSourceSessions");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Consumptions");
        }
    }
}
