import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface StockDto extends FullAuditedEntityDto<string> {
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string;
  expiryDate?: string | null;
  quantity?: number;
  unitCost?: number;
}
