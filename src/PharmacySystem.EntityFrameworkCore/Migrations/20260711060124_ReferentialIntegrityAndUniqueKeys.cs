using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacySystem.Migrations
{
    /// <inheritdoc />
    public partial class ReferentialIntegrityAndUniqueKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppSales_CustomerId",
                table: "AppSales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSales_SaleDate",
                table: "AppSales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppSales_SaleNumber",
                table: "AppSales",
                column: "SaleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSaleItems_MedicineId",
                table: "AppSaleItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPurchases_PurchaseDate",
                table: "AppPurchases",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppPurchases_PurchaseNumber",
                table: "AppPurchases",
                column: "PurchaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPurchases_SupplierId",
                table: "AppPurchases",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPurchaseItems_MedicineId",
                table: "AppPurchaseItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMedicines_Barcode",
                table: "AppMedicines",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_PatientCode",
                table: "AppCustomers",
                column: "PatientCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPurchaseItems_AppMedicines_MedicineId",
                table: "AppPurchaseItems",
                column: "MedicineId",
                principalTable: "AppMedicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPurchases_AppSuppliers_SupplierId",
                table: "AppPurchases",
                column: "SupplierId",
                principalTable: "AppSuppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSaleItems_AppMedicines_MedicineId",
                table: "AppSaleItems",
                column: "MedicineId",
                principalTable: "AppMedicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSales_AppCustomers_CustomerId",
                table: "AppSales",
                column: "CustomerId",
                principalTable: "AppCustomers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStocks_AppMedicines_MedicineId",
                table: "AppStocks",
                column: "MedicineId",
                principalTable: "AppMedicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppPurchaseItems_AppMedicines_MedicineId",
                table: "AppPurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppPurchases_AppSuppliers_SupplierId",
                table: "AppPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSaleItems_AppMedicines_MedicineId",
                table: "AppSaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSales_AppCustomers_CustomerId",
                table: "AppSales");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStocks_AppMedicines_MedicineId",
                table: "AppStocks");

            migrationBuilder.DropIndex(
                name: "IX_AppSales_CustomerId",
                table: "AppSales");

            migrationBuilder.DropIndex(
                name: "IX_AppSales_SaleDate",
                table: "AppSales");

            migrationBuilder.DropIndex(
                name: "IX_AppSales_SaleNumber",
                table: "AppSales");

            migrationBuilder.DropIndex(
                name: "IX_AppSaleItems_MedicineId",
                table: "AppSaleItems");

            migrationBuilder.DropIndex(
                name: "IX_AppPurchases_PurchaseDate",
                table: "AppPurchases");

            migrationBuilder.DropIndex(
                name: "IX_AppPurchases_PurchaseNumber",
                table: "AppPurchases");

            migrationBuilder.DropIndex(
                name: "IX_AppPurchases_SupplierId",
                table: "AppPurchases");

            migrationBuilder.DropIndex(
                name: "IX_AppPurchaseItems_MedicineId",
                table: "AppPurchaseItems");

            migrationBuilder.DropIndex(
                name: "IX_AppMedicines_Barcode",
                table: "AppMedicines");

            migrationBuilder.DropIndex(
                name: "IX_AppCustomers_PatientCode",
                table: "AppCustomers");
        }
    }
}
