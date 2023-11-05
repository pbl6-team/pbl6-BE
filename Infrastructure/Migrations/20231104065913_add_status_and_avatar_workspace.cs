using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_status_and_avatar_workspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvartarUrl",
                schema: "Chat",
                table: "Workspaces",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                schema: "Chat",
                table: "WorkspaceMembers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Status",
                schema: "Chat",
                table: "ChanelMembers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvartarUrl",
                schema: "Chat",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Chat",
                table: "WorkspaceMembers");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Chat",
                table: "ChanelMembers");
        }
    }
}
