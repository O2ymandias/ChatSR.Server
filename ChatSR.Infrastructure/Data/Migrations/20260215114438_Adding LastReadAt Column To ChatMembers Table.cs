using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatSR.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingLastReadAtColumnToChatMembersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReadAt",
                table: "ChatMembers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "ChatMembers");
        }
    }
}
