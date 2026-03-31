import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { PagedAndSortedResultRequestDto } from '@abp/ng.core';

import { DashboardService, DashboardStatsDto } from '../proxy/dashboard';
import { PurchaseDto, PurchaseService } from '../proxy/purchases';
import { SaleDto, SaleService } from '../proxy/sales';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  imports: [CommonModule, BaseChartDirective],
})
export class DashboardComponent implements OnInit {
  stats = {} as DashboardStatsDto;

  recentPurchases: PurchaseDto[] = [];
  recentSales: SaleDto[] = [];

  private readonly dashboardService = inject(DashboardService);
  private readonly purchaseService = inject(PurchaseService);
  private readonly saleService = inject(SaleService);

  salesPurchasesChartData: any = {
    labels: [],
    datasets: [
      { data: [], label: 'Purchases' },
      { data: [], label: 'Sales' }
    ]
  };

  salesPurchasesChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'top' }
    },
    scales: {
      y: { beginAtZero: true }
    }
  };

  topMedicinesChartData: any = {
    labels: [],
    datasets: [
      { data: [], label: 'Quantity Sold' }
    ]
  };

  topMedicinesChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: { position: 'top' }
    },
    scales: {
      x: {
        beginAtZero: true,
        ticks: { precision: 0 }
      }
    }
  };

  stockByCategoryChartData: any = {
    labels: [],
    datasets: [
      { data: [] }
    ]
  };

  stockByCategoryChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '65%',
    plugins: {
      legend: { position: 'bottom' }
    }
  };

  expiryTimelineChartData: any = {
    labels: [],
    datasets: [
      { data: [], label: 'Stock Quantity' }
    ]
  };

  expiryTimelineChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'top' }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { precision: 0 }
      }
    }
  };

  ngOnInit(): void {
    this.loadStats();
    this.loadSalesPurchasesChart();
    this.loadTopMedicinesChart();
    this.loadStockByCategoryChart();
    this.loadExpiryTimelineChart();
    this.loadRecentPurchases();
    this.loadRecentSales();
  }

  loadStats(): void {
    this.dashboardService.getStats().subscribe(response => {
      this.stats = response;
    });
  }

  loadSalesPurchasesChart(): void {
    this.dashboardService.getSalesPurchasesTrend().subscribe(data => {
      this.salesPurchasesChartData = {
        labels: data.map(x => x.label),
        datasets: [
          { data: data.map(x => x.purchaseAmount), label: 'Purchases' },
          { data: data.map(x => x.salesAmount), label: 'Sales' }
        ]
      };
    });
  }

  loadTopMedicinesChart(): void {
    this.dashboardService.getTopSellingMedicines().subscribe(data => {
      this.topMedicinesChartData = {
        labels: data.map(x => x.medicineName),
        datasets: [
          { data: data.map(x => x.totalQuantitySold), label: 'Quantity Sold' }
        ]
      };
    });
  }

  loadStockByCategoryChart(): void {
    this.dashboardService.getStockByCategory().subscribe(data => {
      this.stockByCategoryChartData = {
        labels: data.map(x => x.categoryName),
        datasets: [
          { data: data.map(x => x.totalQuantity) }
        ]
      };
    });
  }

  loadExpiryTimelineChart(): void {
    this.dashboardService.getExpiryTimeline().subscribe(data => {
      this.expiryTimelineChartData = {
        labels: data.map(x => x.label),
        datasets: [
          { data: data.map(x => x.totalQuantity), label: 'Stock Quantity' }
        ]
      };
    });
  }

  loadRecentPurchases(): void {
    const input: PagedAndSortedResultRequestDto = {
      skipCount: 0,
      maxResultCount: 5,
      sorting: ''
    };

    this.purchaseService.getList(input).subscribe(response => {
      this.recentPurchases = response.items ?? [];
    });
  }

  loadRecentSales(): void {
    const input: PagedAndSortedResultRequestDto = {
      skipCount: 0,
      maxResultCount: 5,
      sorting: ''
    };

    this.saleService.getList(input).subscribe(response => {
      this.recentSales = response.items ?? [];
    });
  }
}