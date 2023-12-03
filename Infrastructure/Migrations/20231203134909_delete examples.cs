using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deleteexamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropTable(
                name: "Examples");

            migrationBuilder.DropIndex(
                name: "IX_ChannelMembers_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropColumn(
                name: "ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembers_RoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_RoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "RoleId",
                principalSchema: "Chat",
                principalTable: "ChannelRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_RoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChannelMembers_RoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Examples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examples", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembers_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChannelRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChannelRoleId",
                principalSchema: "Chat",
                principalTable: "ChannelRoles",
                principalColumn: "Id");
        }
    }
}
