import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateSupplierDto {
  name: string;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive?: boolean;
}

export interface SupplierDto extends FullAuditedEntityDto<string> {
  name?: string;
  contactPerson?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive?: boolean;
}
