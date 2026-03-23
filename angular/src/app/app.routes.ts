import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'categories',
    loadComponent: () =>
      import('./categories/category.component').then(c => c.CategoryComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Categories',
    },
  },
  {
    path: 'medicines',
    loadComponent: () =>
      import('./medicines/medicine.component').then(c => c.MedicineComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Medicines',
    },
  },
  {
    path: 'suppliers',
    loadComponent: () =>
      import('./supplier/supplier.component').then(c => c.SupplierComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Suppliers',
    },
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./customers/customer.component').then(c => c.CustomerComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Customers',
    },
  },
  {
    path: 'purchases',
    loadComponent: () =>
      import('./purchases/purchases.component').then(c => c.PurchaseComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Purchases',
    },
  },
  {
    path: 'sales',
    loadComponent: () =>
      import('./sales/sale.component').then(c => c.SaleComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Sales',
    },
  },
  {
    path: 'stocks',
    loadComponent: () =>
      import('./stocks/stock.component').then(c => c.StockComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'PharmacySystem.Stock',
    },
  },
];