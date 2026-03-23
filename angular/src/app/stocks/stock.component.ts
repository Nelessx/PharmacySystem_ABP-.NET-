import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';

import {
  ExpiringStockDto,
  LowStockDto,
  StockDto,
  StockService,
} from '../proxy/stocks';

@Component({
  selector: 'app-stock',
  standalone: true,
  templateUrl: './stock.component.html',
  imports: [CommonModule, ThemeSharedModule],
  providers: [ListService],
})
export class StockComponent implements OnInit {
  // Main stock list
  stocks = { items: [], totalCount: 0 } as PagedResultDto<StockDto>;

  // Low stock list
  lowStockItems: LowStockDto[] = [];

  // Expiring/expired stock list
  expiringStockItems: ExpiringStockDto[] = [];

  public readonly list = inject(ListService);
  private readonly stockService = inject(StockService);

  ngOnInit(): void {
    // Load normal stock list
    const stockStreamCreator = query =>
      this.stockService.getList({
        ...query,
        maxResultCount: 100,
      });

    this.list.hookToQuery(stockStreamCreator).subscribe(response => {
      this.stocks = response;
    });

    // Load low stock items
    this.loadLowStock();

    // Load expiring stock items
    this.loadExpiringStock();
  }

  // Loads stock rows where quantity is below or equal to reorder level
  loadLowStock(): void {
    this.stockService.getLowStock().subscribe(response => {
      this.lowStockItems = response.items;
    });
  }

  // Loads stock rows that are expired or expiring soon
  loadExpiringStock(): void {
    this.stockService.getExpiringStock(30).subscribe(response => {
      this.expiringStockItems = response.items;
    });
  }

  // Returns true if expiry date is already past
  isExpired(expiryDate?: string): boolean {
    if (!expiryDate) return false;

    const today = new Date();
    const expiry = new Date(expiryDate);

    today.setHours(0, 0, 0, 0);
    expiry.setHours(0, 0, 0, 0);

    return expiry < today;
  }

  // Returns true if item is expiring within next 30 days
  isExpiringSoon(expiryDate?: string): boolean {
    if (!expiryDate) return false;

    const today = new Date();
    const expiry = new Date(expiryDate);

    today.setHours(0, 0, 0, 0);
    expiry.setHours(0, 0, 0, 0);

    const diffInMs = expiry.getTime() - today.getTime();
    const diffInDays = diffInMs / (1000 * 60 * 60 * 24);

    return diffInDays >= 0 && diffInDays <= 30;
  }
}