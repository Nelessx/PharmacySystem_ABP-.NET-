import type { DashboardStatsDto, ExpiryTimelineDto, SalesPurchasesTrendPointDto, StockByCategoryDto, TopSellingMedicineDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getExpiryTimeline = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExpiryTimelineDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/expiry-timeline',
    },
    { apiName: this.apiName,...config });
  

  getSalesPurchasesTrend = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesPurchasesTrendPointDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/sales-purchases-trend',
    },
    { apiName: this.apiName,...config });
  

  getStats = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardStatsDto>({
      method: 'GET',
      url: '/api/app/dashboard/stats',
    },
    { apiName: this.apiName,...config });
  

  getStockByCategory = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockByCategoryDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/stock-by-category',
    },
    { apiName: this.apiName,...config });
  

  getTopSellingMedicines = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, TopSellingMedicineDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/top-selling-medicines',
    },
    { apiName: this.apiName,...config });
}