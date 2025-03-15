using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelloChat.Migrations
{
    /// <inheritdoc />
    public partial class DoubleTheReactionPropertyInMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reaction",
                table: "Messages",
                newName: "ReactionFromSender");

            migrationBuilder.AddColumn<int>(
                name: "ReactionFromReceiver",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReactionFromReceiver",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ReactionFromSender",
                table: "Messages",
                newName: "Reaction");
        }
    }
}
