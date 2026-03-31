
export interface DashboardStatsDto {
  totalMedicines?: number;
  totalSuppliers?: number;
  totalCustomers?: number;
  totalPurchases?: number;
  totalSales?: number;
  totalStockRows?: number;
  lowStockCount?: number;
  expiringStockCount?: number;
}

export interface ExpiryTimelineDto {
  label?: string;
  totalQuantity?: number;
}

export interface SalesPurchasesTrendPointDto {
  label?: string;
  purchaseAmount?: number;
  salesAmount?: number;
}

export interface StockByCategoryDto {
  categoryName?: string;
  totalQuantity?: number;
}

export interface TopSellingMedicineDto {
  medicineName?: string;
  totalQuantitySold?: number;
}
