using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacySystem.Migrations
{
    /// <inheritdoc />
    public partial class StockLotUniqueByExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStocks_MedicineId_BatchNumber",
                table: "AppStocks");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_MedicineId_BatchNumber_ExpiryDate",
                table: "AppStocks",
                columns: new[] { "MedicineId", "BatchNumber", "ExpiryDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStocks_MedicineId_BatchNumber_ExpiryDate",
                table: "AppStocks");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_MedicineId_BatchNumber",
                table: "AppStocks",
                columns: new[] { "MedicineId", "BatchNumber" },
                unique: true);
        }
    }
}
