using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockpilot.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStockMovementUuidToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PerformedByUser",
                table: "StockMovements",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PerformedByUser",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
