using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace libranet.Migrations
{
    /// <inheritdoc />
    public partial class SeparateNombreApellido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Socios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Socios");
        }
    }
}
