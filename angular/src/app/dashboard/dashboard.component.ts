import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { DashboardService, DashboardStatsDto } from '../proxy/dashboard';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  imports: [CommonModule, ThemeSharedModule],
})
export class DashboardComponent implements OnInit {
  stats = {} as DashboardStatsDto;
  private readonly dashboardService = inject(DashboardService);

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.dashboardService.getStats().subscribe(response => {
      this.stats = response;
    });
  }
}