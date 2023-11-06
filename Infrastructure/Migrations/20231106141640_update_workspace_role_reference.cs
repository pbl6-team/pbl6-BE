using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_workspace_role_reference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers",
                column: "RoleId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_RoleId",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_WorkspaceRoles_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceMembers",
                column: "WorkspaceId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id");
        }
    }
}
