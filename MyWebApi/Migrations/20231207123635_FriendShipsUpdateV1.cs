using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWebApi.Migrations
{
    /// <inheritdoc />
    public partial class FriendShipsUpdateV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Users_UserId",
                table: "Friendship");

            migrationBuilder.DropIndex(
                name: "IX_Friendship_UserId_FriendId",
                table: "Friendship");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Friendship",
                newName: "UpdatedBy");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Friendship",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Friendship",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Friendship",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_CreatedBy_FriendId",
                table: "Friendship",
                columns: new[] { "CreatedBy", "FriendId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Users_CreatedBy",
                table: "Friendship",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendship_Users_CreatedBy",
                table: "Friendship");

            migrationBuilder.DropIndex(
                name: "IX_Friendship_CreatedBy_FriendId",
                table: "Friendship");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Friendship");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Friendship");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Friendship");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Friendship",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_UserId_FriendId",
                table: "Friendship",
                columns: new[] { "UserId", "FriendId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendship_Users_UserId",
                table: "Friendship",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
