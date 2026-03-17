import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CategoryLookupDto {
  id?: string;
  name?: string;
}

export interface CreateUpdateMedicineDto {
  name: string;
  genericName?: string | null;
  categoryId: string;
  unit?: string | null;
  barcode?: string | null;
  purchasePrice?: number;
  salePrice?: number;
  reorderLevel?: number;
  isActive?: boolean;
}

export interface MedicineDto extends FullAuditedEntityDto<string> {
  name?: string;
  genericName?: string | null;
  categoryId?: string;
  categoryName?: string | null;
  unit?: string | null;
  barcode?: string | null;
  purchasePrice?: number;
  salePrice?: number;
  reorderLevel?: number;
  isActive?: boolean;
}
