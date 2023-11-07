using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChannelId",
                schema: "Chat",
                table: "ChannelRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceRoles_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoles_ChannelId",
                schema: "Chat",
                table: "ChannelRoles",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelRoles_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelRoles",
                column: "ChannelId",
                principalSchema: "Chat",
                principalTable: "Channels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceRoles_Workspaces_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles",
                column: "WorkspaceId",
                principalSchema: "Chat",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelRoles_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceRoles_Workspaces_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceRoles_WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropIndex(
                name: "IX_ChannelRoles_ChannelId",
                schema: "Chat",
                table: "ChannelRoles");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                schema: "Chat",
                table: "WorkspaceRoles");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                schema: "Chat",
                table: "ChannelRoles");
        }
    }
}
