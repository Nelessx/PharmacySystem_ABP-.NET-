import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface ExpiringStockDto {
  stockId?: string;
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string;
  expiryDate?: string | null;
  quantity?: number;
  daysToExpire?: number;
  isExpired?: boolean;
}

export interface LowStockDto {
  stockId?: string;
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string;
  expiryDate?: string | null;
  quantity?: number;
  reorderLevel?: number;
}

export interface StockDto extends FullAuditedEntityDto<string> {
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string;
  expiryDate?: string | null;
  quantity?: number;
  unitCost?: number;
}
