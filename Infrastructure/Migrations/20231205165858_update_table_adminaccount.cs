using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_table_adminaccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OTP",
                schema: "Admin",
                table: "AdminTokens",
                newName: "Otp");

            migrationBuilder.RenameColumn(
                name: "UserName",
                schema: "Admin",
                table: "AdminAccounts",
                newName: "Username");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                schema: "Admin",
                table: "AdminTokens",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "OtpType",
                schema: "Admin",
                table: "AdminTokens",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefreshTokenTimeOut",
                schema: "Admin",
                table: "AdminTokens",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpType",
                schema: "Admin",
                table: "AdminTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenTimeOut",
                schema: "Admin",
                table: "AdminTokens");

            migrationBuilder.RenameColumn(
                name: "Otp",
                schema: "Admin",
                table: "AdminTokens",
                newName: "OTP");

            migrationBuilder.RenameColumn(
                name: "Username",
                schema: "Admin",
                table: "AdminAccounts",
                newName: "UserName");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                schema: "Admin",
                table: "AdminTokens",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
