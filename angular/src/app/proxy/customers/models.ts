import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateCustomerDto {
  name: string;
  phone?: string | null;
  address?: string | null;
  gender?: string | null;
  dateOfBirth?: string | null;
  patientCode?: string | null;
  isActive?: boolean;
}

export interface CustomerDto extends FullAuditedEntityDto<string> {
  name?: string;
  phone?: string | null;
  address?: string | null;
  gender?: string | null;
  dateOfBirth?: string | null;
  patientCode?: string | null;
  isActive?: boolean;
}
