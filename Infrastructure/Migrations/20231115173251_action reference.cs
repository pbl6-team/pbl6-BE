using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class actionreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers",
                column: "RoleId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers",
                column: "RoleId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id");
        }
    }
}
