using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_refreshtokentimeout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTP",
                schema: "User",
                table: "UserTokens",
                newName: "Otp");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefreshTokenTimeOut",
                schema: "User",
                table: "UserTokens",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenTimeOut",
                schema: "User",
                table: "UserTokens");

            migrationBuilder.RenameColumn(
                name: "Otp",
                schema: "User",
                table: "UserTokens",
                newName: "OTP");
        }
    }
}
