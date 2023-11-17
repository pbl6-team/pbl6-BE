using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatereference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionsOfChannelRoles_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissionsOfWorkspaceRoles_WorkspaceRoles_WorkspaceRoleId",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles");

            migrationBuilder.RenameColumn(
                name: "IsEnable",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "IsEnable",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                newName: "IsEnabled");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceRoles_WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles",
                column: "WorkspacePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoles_ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles",
                column: "ChannelPermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelRoles_ChannelPermissions_ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles",
                column: "ChannelPermissionId",
                principalSchema: "Chat",
                principalTable: "ChannelPermissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionsOfChannelRoles_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                column: "ChannelRoleId",
                principalSchema: "Chat",
                principalTable: "ChannelRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionsOfWorkspaceRoles_WorkspaceRoles_WorkspaceRoleId",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles",
                column: "WorkspaceRoleId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceRoles_WorkspacePermissions_WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles",
                column: "WorkspacePermissionId",
                principalSchema: "Chat",
                principalTable: "WorkspacePermissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelRoles_ChannelPermissions_ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissionsOfChannelRoles_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissionsOfWorkspaceRoles_WorkspaceRoles_WorkspaceRoleId",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceRoles_WorkspacePermissions_WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceRoles_WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropIndex(
                name: "IX_ChannelRoles_ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles");

            migrationBuilder.DropColumn(
                name: "WorkspacePermissionId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropColumn(
                name: "ChannelPermissionId",
                schema: "Chat",
                table: "ChannelRoles");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles",
                newName: "IsEnable");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                newName: "IsEnable");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionsOfChannelRoles_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                column: "ChannelRoleId",
                principalSchema: "Chat",
                principalTable: "ChannelRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionsOfWorkspaceRoles_WorkspaceRoles_WorkspaceRoleId",
                schema: "Chat",
                table: "PermissionsOfWorkspaceRoles",
                column: "WorkspaceRoleId",
                principalSchema: "Chat",
                principalTable: "WorkspaceRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
