# PharmacySystem Frontend Implementation Summary

**Project:** PharmacySystem  
**Frontend location:** `angular/`  
**UI framework:** Angular with ABP Angular packages  
**Theme:** ABP LeptonX side-menu layout  
**Last reviewed:** 2026-05-30  

---

## Direct Answers

### Are you using ABP MVC/Razor Pages, Blazor, Angular, or React for UI?

This project is using **Angular** for the UI.

Evidence:

- The frontend application is in the `angular/` folder.
- The Angular app uses `@angular/*` packages around Angular 20.
- It uses ABP Angular packages such as `@abp/ng.core`, `@abp/ng.account`, `@abp/ng.identity`, `@abp/ng.tenant-management`, and `@abp/ng.theme.lepton-x`.
- There is no MVC/Razor Pages UI, Blazor UI, or React UI implemented as the active frontend.

### Does your project currently have any UI pages, or only API/backend?

The project currently has **real Angular UI pages**, not only API/backend.

Implemented Angular pages include:

- Home page
- Dashboard page
- Category management page
- Medicine management page
- Supplier management page
- Customer management page
- Purchase management page
- Sales management page
- Stock page
- ABP built-in Account pages
- ABP built-in Identity pages
- ABP built-in Tenant Management pages
- ABP built-in Setting Management pages

The backend/API is also implemented, but the Angular frontend is already connected to generated ABP proxy services and has working screens for the pharmacy modules.

### Do you want the UI to be a normal admin dashboard or a POS-style pharmacy screen?

Based on the current implementation, the UI is a **normal admin dashboard / management system**.

It has menu-based pages, tables, CRUD modals, dashboard cards, charts, and management screens. It is not currently a dedicated POS-style pharmacy counter screen with barcode-first workflow, fast cart checkout, receipt printing, payment flow, or cashier-focused layout.

Current sales functionality exists, but it is implemented as an admin-style sales CRUD page rather than a full POS checkout screen.

### Are you using ABP's built-in theme, like LeptonX, or a custom Bootstrap theme?

The project is using **ABP's built-in LeptonX theme** with the side-menu layout.

Evidence from `angular/src/app/app.config.ts`:

- `provideThemeLeptonX()`
- `provideSideMenuLayout()`
- `provideAbpThemeShared()`
- `provideLogo(withEnvironmentOptions(environment))`

The pages also use Bootstrap-style classes and ABP shared components such as `abp-modal`, but the active application theme is LeptonX, not a fully custom Bootstrap-only theme.

---

## Frontend Stack

The Angular frontend uses:

- Angular 20
- ABP Angular 10 packages
- LeptonX theme package `@abp/ng.theme.lepton-x`
- ABP OAuth integration through `@abp/ng.oauth`
- ABP account, identity, tenant management, setting management, and feature management modules
- Reactive Forms for create/edit screens
- ABP `ListService` for list loading and refresh behavior
- ABP `RestService` generated proxies for backend API calls
- ABP `ConfirmationService` for delete confirmations
- `ng2-charts` and `chart.js` for dashboard charts

Main frontend files:

- `angular/src/app/app.config.ts`
- `angular/src/app/app.routes.ts`
- `angular/src/app/route.provider.ts`
- `angular/src/environments/environment.ts`
- `angular/src/app/proxy/`

---

## Application Configuration

The Angular app is configured as an ABP Angular application.

Current environment settings:

- Frontend base URL: `http://localhost:4200`
- Backend/API URL: `https://localhost:44378`
- OAuth issuer: `https://localhost:44378/`
- OAuth client ID: `PharmacySystem_App`
- OAuth flow: authorization code flow
- Scope: `offline_access PharmacySystem`
- HTTPS required for authentication

Configured providers include:

- Angular router
- Angular animations
- ABP core
- ABP OAuth
- ABP account
- ABP identity
- ABP setting management
- ABP feature management
- ABP tenant management
- ABP LeptonX theme
- ABP side-menu layout
- ABP shared theme services
- Chart.js registration through `ng2-charts`

---

## Routing and Navigation

Routes are defined in `angular/src/app/app.routes.ts`.

Custom pharmacy routes:

| Route | Component | Protection |
| --- | --- | --- |
| `/` | HomeComponent | Public/home route |
| `/dashboard` | DashboardComponent | No explicit permission guard currently |
| `/categories` | CategoryComponent | Auth + `PharmacySystem.Categories` |
| `/medicines` | MedicineComponent | Auth + `PharmacySystem.Medicines` |
| `/suppliers` | SupplierComponent | Auth + `PharmacySystem.Suppliers` |
| `/customers` | CustomerComponent | Auth + `PharmacySystem.Customers` |
| `/purchases` | PurchaseComponent | Auth + `PharmacySystem.Purchases` |
| `/sales` | SaleComponent | Auth + `PharmacySystem.Sales` |
| `/stocks` | StockComponent | Auth + `PharmacySystem.Stock` |

ABP built-in module routes:

| Route | Module |
| --- | --- |
| `/account` | ABP Account module |
| `/identity` | ABP Identity module |
| `/tenant-management` | ABP Tenant Management module |
| `/setting-management` | ABP Setting Management module |

Menu entries are registered in `angular/src/app/route.provider.ts` using ABP `RoutesService`.

Custom menu entries:

- Home
- Dashboard
- Categories
- Medicines
- Suppliers
- Customers
- Purchases
- Sales
- Stock

Most pharmacy menu entries are permission-aware through `requiredPolicy`.

---

## Generated API Proxies

The Angular app uses generated ABP proxy services under `angular/src/app/proxy/`.

Generated proxy modules:

- `categories`
- `medicines`
- `suppliers`
- `customers`
- `purchases`
- `sales`
- `stocks`
- `dashboard`

These services call backend endpoints using ABP `RestService`.

Examples:

- `CategoryService`
- `MedicineService`
- `SupplierService`
- `CustomerService`
- `PurchaseService`
- `SaleService`
- `StockService`
- `DashboardService`

The frontend is therefore not manually hardcoding raw `HttpClient` calls in each page. It mainly consumes the generated service layer.

---

## Pages Implemented

### Home Page

Files:

- `angular/src/app/home/home.component.ts`
- `angular/src/app/home/home.component.html`
- `angular/src/app/home/home.component.scss`

Current status:

- Uses the default ABP getting-started style content.
- Shows login action when the user is not logged in.
- Links to ABP documentation, support, blog, community, samples, and related resources.

Note:

- This page is still mostly the default ABP starter home page and has not yet been converted into a pharmacy-specific landing/dashboard page.

### Dashboard Page

Files:

- `angular/src/app/dashboard/dashboard.component.ts`
- `angular/src/app/dashboard/dashboard.component.html`
- `angular/src/app/dashboard/dashboard.component.scss`

Implemented work:

- Pharmacy dashboard overview page.
- KPI/stat cards for:
  - Medicines
  - Suppliers
  - Customers
  - Stock rows
  - Low stock
  - Expiring stock
  - Purchases
  - Sales
- Inventory health summary.
- Total operational records summary.
- Sales vs purchases chart.
- Top-selling medicines chart.
- Stock distribution by category chart.
- Expiry timeline chart.
- Recent purchases table.
- Recent sales table.

Backend services used:

- `DashboardService.getStats()`
- `DashboardService.getSalesPurchasesTrend()`
- `DashboardService.getTopSellingMedicines()`
- `DashboardService.getStockByCategory()`
- `DashboardService.getExpiryTimeline()`
- `PurchaseService.getList()`
- `SaleService.getList()`

Charts:

- Implemented with `ng2-charts` / Chart.js.
- Uses bar and doughnut charts.

### Category Management Page

Files:

- `angular/src/app/categories/category.component.ts`
- `angular/src/app/categories/category.component.html`

Implemented work:

- Category list table.
- Create category modal.
- Edit category modal.
- Delete category with ABP confirmation dialog.
- Reactive form fields:
  - Name
  - Description
  - Active/inactive flag
- Validation:
  - Name is required.
- Uses `CategoryService` generated proxy.
- Uses `ListService` to reload data after changes.

### Medicine Management Page

Files:

- `angular/src/app/medicines/medicine.component.ts`
- `angular/src/app/medicines/medicine.component.html`

Implemented work:

- Medicine list table.
- Create medicine modal.
- Edit medicine modal.
- Delete medicine with confirmation.
- Category dropdown lookup.
- Reactive form fields:
  - Name
  - Generic name
  - Category
  - Unit
  - Barcode
  - Purchase price
  - Sale price
  - Reorder level
  - Active/inactive flag
- Validation:
  - Required name
  - Required category
  - Required purchase price
  - Required sale price
  - Required reorder level
  - Max length checks
  - Minimum numeric values for price and reorder level
- Uses `MedicineService`.
- Uses `MedicineService.getCategoryLookup()` for category dropdown data.

### Supplier Management Page

Files:

- `angular/src/app/supplier/supplier.component.ts`
- `angular/src/app/supplier/supplier.component.html`

Implemented work:

- Supplier list table.
- Create supplier modal.
- Edit supplier modal.
- Delete supplier with confirmation.
- Reactive form fields:
  - Name
  - Contact person
  - Phone
  - Email
  - Address
  - Active/inactive flag
- Validation:
  - Name required
  - Email format validation
  - Max length checks
- Uses `SupplierService`.

### Customer Management Page

Files:

- `angular/src/app/customers/customer.component.ts`
- `angular/src/app/customers/customer.component.html`

Implemented work:

- Customer list table.
- Create customer modal.
- Edit customer modal.
- Delete customer with confirmation.
- Reactive form fields:
  - Name
  - Phone
  - Gender
  - Date of birth
  - Patient code
  - Address
  - Active/inactive flag
- Date conversion from date input to ISO string before saving.
- Validation:
  - Name required
  - Max length checks
- Uses `CustomerService`.

### Purchase Management Page

Files:

- `angular/src/app/purchases/purchases.component.ts`
- `angular/src/app/purchases/purchase.component.html`

Implemented work:

- Purchase list table.
- Create purchase modal.
- Edit purchase modal.
- Delete purchase with confirmation.
- XL modal for purchase entry.
- Supplier dropdown lookup.
- Medicine dropdown lookup.
- Dynamic purchase item rows using `FormArray`.
- Add/remove item row behavior.
- At least one item row is kept.
- UI-only line total calculation.
- UI-only total amount and net amount calculation.
- Discount amount included in total calculation.
- Date conversion from date input to ISO string before saving.
- Concurrency stamp included for update operations.

Purchase header fields:

- Purchase number
- Supplier
- Purchase date
- Invoice number
- Discount amount
- Notes

Purchase item fields:

- Medicine
- Batch number
- Expiry date
- Quantity
- Unit price
- Line total

Backend services used:

- `PurchaseService.getList()`
- `PurchaseService.get()`
- `PurchaseService.create()`
- `PurchaseService.update()`
- `PurchaseService.delete()`
- `PurchaseService.getSupplierLookup()`
- `PurchaseService.getMedicineLookup()`

Important note:

- The purchase frontend sends batch number and expiry date so the backend can update stock at batch/expiry level.

### Sales Management Page

Files:

- `angular/src/app/sales/sale.component.ts`
- `angular/src/app/sales/sale.component.html`

Implemented work:

- Sales list table.
- Create sale modal.
- Edit sale modal.
- Delete sale with confirmation.
- XL modal for sale entry.
- Customer dropdown lookup.
- Medicine dropdown lookup.
- Dynamic sale item rows using `FormArray`.
- Add/remove item row behavior.
- At least one item row is kept.
- UI-only line total calculation.
- UI-only total amount and net amount calculation.
- Discount amount included in total calculation.
- Date conversion from date input to ISO string before saving.
- Optional customer support for walk-in sales.

Sale header fields:

- Sale number
- Customer
- Sale date
- Discount amount
- Notes

Sale item fields:

- Medicine
- Batch number
- Quantity
- Unit price
- Line total

Backend services used:

- `SaleService.getList()`
- `SaleService.get()`
- `SaleService.create()`
- `SaleService.update()`
- `SaleService.delete()`
- `SaleService.getCustomerLookup()`
- `SaleService.getMedicineLookup()`

Important note:

- This is a sales management CRUD screen, not a fully optimized POS screen yet.

### Stock Page

Files:

- `angular/src/app/stocks/stock.component.ts`
- `angular/src/app/stocks/stock.component.html`

Implemented work:

- Main stock list table.
- Low stock table/list.
- Expiring or expired stock table/list.
- Loads stock rows with batch and expiry information.
- Loads low stock records from backend.
- Loads expiring stock records using a 30-day window.
- Contains frontend helper methods:
  - `isExpired(expiryDate)`
  - `isExpiringSoon(expiryDate)`

Backend services used:

- `StockService.getList()`
- `StockService.getLowStock()`
- `StockService.getExpiringStock(30)`

---

## Authentication and Authorization

The Angular app uses ABP OAuth and ABP route guards.

Implemented frontend authorization:

- `authGuard` protects custom pharmacy module pages.
- `permissionGuard` checks ABP permissions on most custom routes.
- Menu items use `requiredPolicy` so users only see/enter pages allowed by their permissions.

Permission policies used in the Angular routes:

- `PharmacySystem.Categories`
- `PharmacySystem.Medicines`
- `PharmacySystem.Suppliers`
- `PharmacySystem.Customers`
- `PharmacySystem.Purchases`
- `PharmacySystem.Sales`
- `PharmacySystem.Stock`

Dashboard currently does not have an explicit route permission guard in `app.routes.ts`.

---

## UI Pattern Used

The current UI pattern is:

- Admin dashboard layout
- Left side menu from LeptonX
- Table-based list pages
- Modal-based create/edit forms
- Delete confirmation dialogs
- Reactive forms with validators
- Dropdown lookups for related records
- Dashboard cards and charts for analytics

This matches a pharmacy admin/management system more than a POS terminal workflow.

---

## Backend Features Reflected in the Frontend

The frontend connects to backend modules for:

- Categories
- Medicines
- Suppliers
- Customers
- Purchases
- Sales
- Stock
- Dashboard/reporting
- Identity and tenant management through ABP modules

Important backend business features surfaced in the UI:

- Medicine category selection.
- Medicine pricing and reorder level.
- Supplier management for purchases.
- Customer management for sales.
- Purchase entry with multiple line items.
- Sale entry with multiple line items.
- Batch number tracking.
- Expiry date tracking on purchases.
- Stock list display.
- Low stock alerts.
- Expiring stock alerts.
- Dashboard analytics and recent transactions.

---

## Current Frontend Gaps / Not Yet Done

The frontend has useful management screens, but these items are not yet fully implemented:

- Home page is still mostly the ABP starter page.
- No dedicated POS/cashier screen yet.
- No barcode scanner focused workflow yet.
- No cart-style checkout UI yet.
- No payment method or payment status UI.
- No receipt/print invoice UI.
- No advanced filtering/search UI on tables.
- No frontend pagination controls beyond the current `ListService` loading pattern.
- No custom localization entries verified for all menu labels and page text.
- No dedicated frontend unit/e2e tests were reviewed for the custom pharmacy pages.
- Purchase page still contains debug `console.log` statements in `save()`.
- Dashboard route is not currently guarded by a report/dashboard permission in the route definition.

---

## Useful Commands

From `angular/`:

```powershell
yarn install
yarn start
yarn build
yarn build:prod
yarn test
yarn lint
```

From the solution root:

```powershell
dotnet restore
dotnet build
dotnet run --project src/PharmacySystem.DbMigrator
dotnet run --project src/PharmacySystem.HttpApi.Host
```

Expected local URLs based on current config:

- Angular UI: `http://localhost:4200`
- API/auth server: `https://localhost:44378`

---

## Short Project Status

PharmacySystem is an ABP layered application with a .NET backend and an Angular frontend. The backend contains the pharmacy domain, APIs, permissions, stock logic, and dashboard data. The frontend is already more than a placeholder: it has Angular screens for the main pharmacy workflows, ABP generated proxies, authentication, permission-based routes, LeptonX navigation, CRUD modals, stock alert views, and dashboard analytics.

The current UI should be described as an **Angular admin dashboard for pharmacy management**. A **POS-style pharmacy sales screen** can be added later as a separate workflow on top of the existing sales, medicines, customers, and stock APIs.
