using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class change_nmame_for_scheme_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserInfos",
                schema: "Users",
                newName: "UserInfos",
                newSchema: "User");

            migrationBuilder.RenameTable(
                name: "Plans",
                schema: "Users",
                newName: "Plans",
                newSchema: "User");

            migrationBuilder.RenameTable(
                name: "PlanDetails",
                schema: "Users",
                newName: "PlanDetails",
                newSchema: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Users");

            migrationBuilder.RenameTable(
                name: "UserInfos",
                schema: "User",
                newName: "UserInfos",
                newSchema: "Users");

            migrationBuilder.RenameTable(
                name: "Plans",
                schema: "User",
                newName: "Plans",
                newSchema: "Users");

            migrationBuilder.RenameTable(
                name: "PlanDetails",
                schema: "User",
                newName: "PlanDetails",
                newSchema: "Users");
        }
    }
}
