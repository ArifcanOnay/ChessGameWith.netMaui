using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatrancApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTasSimgesiPropery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TasSimgesi",
                table: "Taslar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TasSimgesi",
                table: "Taslar");
        }
    }
}
