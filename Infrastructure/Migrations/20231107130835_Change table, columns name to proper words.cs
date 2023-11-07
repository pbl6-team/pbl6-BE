using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changetablecolumnsnametoproperwords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_ChanelRoles_ChanelRoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropTable(
                name: "PermissionsOfChanelRoles",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChanelPermissions",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChanelRoles",
                schema: "Chat");

            migrationBuilder.DropColumn(
                name: "ChanelId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.RenameColumn(
                name: "AvartarUrl",
                schema: "Chat",
                table: "Workspaces",
                newName: "AvatarUrl");

            migrationBuilder.RenameColumn(
                name: "ChanelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                newName: "ChannelRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelMembers_ChanelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                newName: "IX_ChannelMembers_ChannelRoleId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChannelId",
                schema: "Chat",
                table: "ChannelMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ChannelPermissions",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelRoles",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionsOfChannelRoles",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsOfChannelRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionsOfChannelRoles_ChannelPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "Chat",
                        principalTable: "ChannelPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionsOfChannelRoles_ChannelRoles_ChannelRoleId",
                        column: x => x.ChannelRoleId,
                        principalSchema: "Chat",
                        principalTable: "ChannelRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionsOfChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                column: "ChannelRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionsOfChannelRoles_PermissionId",
                schema: "Chat",
                table: "PermissionsOfChannelRoles",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChannelRoleId",
                principalSchema: "Chat",
                principalTable: "ChannelRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChannelId",
                principalSchema: "Chat",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_ChannelRoles_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelMembers");

            migrationBuilder.DropTable(
                name: "PermissionsOfChannelRoles",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChannelPermissions",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChannelRoles",
                schema: "Chat");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                schema: "Chat",
                table: "Workspaces",
                newName: "AvartarUrl");

            migrationBuilder.RenameColumn(
                name: "ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                newName: "ChanelRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelMembers_ChannelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                newName: "IX_ChannelMembers_ChanelRoleId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChannelId",
                schema: "Chat",
                table: "ChannelMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ChanelId",
                schema: "Chat",
                table: "ChannelMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ChanelPermissions",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChanelPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChanelRoles",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChanelRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionsOfChanelRoles",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChanelRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsOfChanelRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionsOfChanelRoles_ChanelPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "Chat",
                        principalTable: "ChanelPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionsOfChanelRoles_ChanelRoles_ChanelRoleId",
                        column: x => x.ChanelRoleId,
                        principalSchema: "Chat",
                        principalTable: "ChanelRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionsOfChanelRoles_ChanelRoleId",
                schema: "Chat",
                table: "PermissionsOfChanelRoles",
                column: "ChanelRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionsOfChanelRoles_PermissionId",
                schema: "Chat",
                table: "PermissionsOfChanelRoles",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_ChanelRoles_ChanelRoleId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChanelRoleId",
                principalSchema: "Chat",
                principalTable: "ChanelRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                schema: "Chat",
                table: "ChannelMembers",
                column: "ChannelId",
                principalSchema: "Chat",
                principalTable: "Channels",
                principalColumn: "Id");
        }
    }
}
