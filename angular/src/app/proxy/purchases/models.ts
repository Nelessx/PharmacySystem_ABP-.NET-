import type { EntityDto, FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdatePurchaseDto {
  purchaseNumber: string;
  supplierId: string;
  purchaseDate?: string;
  invoiceNumber?: string | null;
  notes?: string | null;
  discountAmount?: number;
  items: CreateUpdatePurchaseItemDto[];
}

export interface CreateUpdatePurchaseItemDto {
  medicineId: string;
  batchNumber?: string | null;
  expiryDate?: string | null;
  quantity?: number;
  unitPrice?: number;
}

export interface MedicineLookupDto {
  id?: string;
  name?: string;
}

export interface PurchaseDto extends FullAuditedEntityDto<string> {
  purchaseNumber?: string;
  supplierId?: string;
  supplierName?: string | null;
  purchaseDate?: string;
  invoiceNumber?: string | null;
  notes?: string | null;
  totalAmount?: number;
  discountAmount?: number;
  netAmount?: number;
  items?: PurchaseItemDto[];
}

export interface PurchaseItemDto extends EntityDto<string> {
  medicineId?: string;
  medicineName?: string | null;
  batchNumber?: string | null;
  expiryDate?: string | null;
  quantity?: number;
  unitPrice?: number;
  lineTotal?: number;
}

export interface SupplierLookupDto {
  id?: string;
  name?: string;
}
