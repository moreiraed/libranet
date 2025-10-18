using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace libranet.Migrations
{
    /// <inheritdoc />
    public partial class AddSocioProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDeAlta",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "FechaDeAlta",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Socios");
        }
    }
}
