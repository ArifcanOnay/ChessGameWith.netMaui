using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatrancApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Taslar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Taslar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Taslar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Taslar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Taslar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Taslar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Taslar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Oyunlar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Oyunlar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Oyunlar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Oyunlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Oyunlar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Oyunlar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Oyunlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Oyuncular",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Oyuncular",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Oyuncular",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Oyuncular",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Oyuncular",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Oyuncular",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Oyuncular",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Hamleler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Hamleler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Hamleler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Hamleler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Hamleler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Hamleler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Hamleler",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Taslar");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Oyunlar");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Oyuncular");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Hamleler");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Hamleler");
        }
    }
}
