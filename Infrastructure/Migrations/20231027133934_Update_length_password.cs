using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_length_password : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "Admin",
                newName: "UserTokens",
                newSchema: "User");

            migrationBuilder.RenameColumn(
                name: "UserName",
                schema: "User",
                table: "Users",
                newName: "Username");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                schema: "User",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "User",
                newName: "UserTokens",
                newSchema: "Admin");

            migrationBuilder.RenameColumn(
                name: "Username",
                schema: "User",
                table: "Users",
                newName: "UserName");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                schema: "User",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
