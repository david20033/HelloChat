using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelloChat.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeleteLocalyProrertyInMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isLocalDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isLocalDeleted",
                table: "Messages");
        }
    }
}
