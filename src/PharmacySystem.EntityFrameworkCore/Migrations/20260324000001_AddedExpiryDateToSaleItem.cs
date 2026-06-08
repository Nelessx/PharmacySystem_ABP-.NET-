using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedExpiryDateToSaleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "AppSaleItems",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "AppSaleItems");
        }
    }
}
