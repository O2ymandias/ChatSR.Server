using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatSR.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigratingintoDateTimeOffsetinsteadofDateTimeintheChatMembermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "JoinedAt",
                table: "ChatMembers",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "ChatMembers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
