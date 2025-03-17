using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelloChat.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertiesForUserActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTimeActive",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTimeActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "AspNetUsers");
        }
    }
}
