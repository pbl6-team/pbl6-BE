﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changecategorytypeforchannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "Chat",
                table: "Channels");

            migrationBuilder.AddColumn<short>(
                name: "Category",
                schema: "Chat",
                table: "Channels",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "Chat",
                table: "Channels");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "Chat",
                table: "Channels",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
