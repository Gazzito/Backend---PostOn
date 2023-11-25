using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebApi.Migrations
{
    /// <inheritdoc />
    public partial class User_Model_Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Logins");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Logins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Salt",
                table: "Logins",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Logins");

            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Logins");

            migrationBuilder.AddColumn<int>(
                name: "Password",
                table: "Logins",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
