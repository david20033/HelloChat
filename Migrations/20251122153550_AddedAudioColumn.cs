using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelloChat.Migrations
{
    /// <inheritdoc />
    public partial class AddedAudioColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AudioFile",
                table: "Messages",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioFile",
                table: "Messages");
        }
    }
}
