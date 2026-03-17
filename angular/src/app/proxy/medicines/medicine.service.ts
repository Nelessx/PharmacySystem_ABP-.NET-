import type { CategoryLookupDto, CreateUpdateMedicineDto, MedicineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MedicineService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateMedicineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDto>({
      method: 'POST',
      url: '/api/app/medicine',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/medicine/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDto>({
      method: 'GET',
      url: `/api/app/medicine/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getCategoryLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CategoryLookupDto>>({
      method: 'GET',
      url: '/api/app/medicine/category-lookup',
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MedicineDto>>({
      method: 'GET',
      url: '/api/app/medicine',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateMedicineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDto>({
      method: 'PUT',
      url: `/api/app/medicine/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}