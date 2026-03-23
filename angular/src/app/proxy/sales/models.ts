import type { EntityDto, FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateSaleDto {
  saleNumber: string;
  customerId?: string | null;
  saleDate?: string;
  notes?: string | null;
  discountAmount?: number;
  items: CreateUpdateSaleItemDto[];
}

export interface CreateUpdateSaleItemDto {
  medicineId: string;
  batchNumber?: string | null;
  quantity?: number;
  unitPrice?: number;
}

export interface CustomerLookupDto {
  id?: string;
  name?: string;
}

export interface MedicineLookupDto {
  id?: string;
  name?: string;
}

export interface SaleDto extends FullAuditedEntityDto<string> {
  saleNumber?: string;
  customerId?: string | null;
  customerName?: string | null;
  saleDate?: string;
  notes?: string | null;
  totalAmount?: number;
  discountAmount?: number;
  netAmount?: number;
  items?: SaleItemDto[];
}

export interface SaleItemDto extends EntityDto<string> {
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string | null;
  quantity?: number;
  unitPrice?: number;
  lineTotal?: number;
}
