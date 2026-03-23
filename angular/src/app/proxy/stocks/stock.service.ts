import type { ExpiringStockDto, LowStockDto, StockDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StockService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockDto>({
      method: 'GET',
      url: `/api/app/stock/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getExpiringStock = (days: number = 30, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ExpiringStockDto>>({
      method: 'GET',
      url: '/api/app/stock/expiring-stock',
      params: { days },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StockDto>>({
      method: 'GET',
      url: '/api/app/stock',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLowStock = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<LowStockDto>>({
      method: 'GET',
      url: '/api/app/stock/low-stock',
    },
    { apiName: this.apiName,...config });
}