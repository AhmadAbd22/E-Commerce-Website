using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceWebsite.Migrations
{
    /// <inheritdoc />
    public partial class RenamedOrderDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderDetails_OrderetailsId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OrderDetails",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "OrderetailsId",
                table: "OrderItems",
                newName: "OrderDetailsId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderetailsId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderDetails_OrderDetailsId",
                table: "OrderItems",
                column: "OrderDetailsId",
                principalTable: "OrderDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderDetails_OrderDetailsId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "OrderDetailsId",
                table: "OrderItems",
                newName: "OrderetailsId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderDetailsId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderetailsId");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderDetails",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderDetails_OrderetailsId",
                table: "OrderItems",
                column: "OrderetailsId",
                principalTable: "OrderDetails",
                principalColumn: "Id");
        }
    }
}
