using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelloChat.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeleteGlobalyProrertyInMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Messages");
        }
    }
}
