using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatSR.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingDisplayPictureUrlColToChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayPictureUrl",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayPictureUrl",
                table: "Chats");
        }
    }
}
