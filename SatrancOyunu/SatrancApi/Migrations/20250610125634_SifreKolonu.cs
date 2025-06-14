using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatrancApi.Migrations
{
    /// <inheritdoc />
    public partial class SifreKolonu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Oyuncular",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Oyuncular");
        }
    }
}
