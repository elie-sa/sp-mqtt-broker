using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SPBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_PolicyTypes_PolicyTypeId",
                table: "Policies");

            migrationBuilder.DropTable(
                name: "PolicyTypes");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PolicyTypeId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "GreaterThan",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "LessThan",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PolicyTypeId",
                table: "Policies");

            migrationBuilder.AddColumn<long>(
                name: "PowerSourceId",
                table: "Policies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TempGreaterThan",
                table: "Policies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TempLessThan",
                table: "Policies",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PowerSourceId",
                table: "Policies",
                column: "PowerSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_PowerSources_PowerSourceId",
                table: "Policies",
                column: "PowerSourceId",
                principalTable: "PowerSources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_PowerSources_PowerSourceId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PowerSourceId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PowerSourceId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "TempGreaterThan",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "TempLessThan",
                table: "Policies");

            migrationBuilder.AddColumn<bool>(
                name: "GreaterThan",
                table: "Policies",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LessThan",
                table: "Policies",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PolicyTypeId",
                table: "Policies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "PolicyTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyTypeId",
                table: "Policies",
                column: "PolicyTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_PolicyTypes_PolicyTypeId",
                table: "Policies",
                column: "PolicyTypeId",
                principalTable: "PolicyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
