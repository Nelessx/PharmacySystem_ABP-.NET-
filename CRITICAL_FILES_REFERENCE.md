# PharmacySystem - Critical Files Reference

## Essential Files for Development

### Solution & Project Files
```
PharmacySystem.sln                          # Main solution file
Directory.Build.props                       # Global build configuration
global.json                                 # Global .NET configuration
```

---

## 🔴 DOMAIN LAYER - Business Rules & Entities

### Entities
- `src/PharmacySystem.Domain/Categories/Category.cs`
- `src/PharmacySystem.Domain/Medicines/Medicine.cs`
- `src/PharmacySystem.Domain/Suppliers/Supplier.cs`
- `src/PharmacySystem.Domain/Customers/Customer.cs`
- `src/PharmacySystem.Domain/Purchases/Purchase.cs` ⭐ (Aggregate root with items)
- `src/PharmacySystem.Domain/Purchases/PurchaseItem.cs` ⭐ (Line items, batch-aware)
- `src/PharmacySystem.Domain/Sales/Sale.cs` ⭐ (Aggregate root with items)
- `src/PharmacySystem.Domain/Sales/SaleItem.cs` ⭐ (Line items, batch-aware)
- `src/PharmacySystem.Domain/Stocks/Stock.cs` ⭐ (Batch + expiry tracking)

### Domain Services
- `src/PharmacySystem.Domain/Stocks/StockManager.cs` ⭐ (Critical: IncreaseAsync, DecreaseAsync)

### Domain Interfaces
- `src/PharmacySystem.Domain/Sales/ISaleRepository.cs` (Custom repository for Sale queries)

### Module Registration
- `src/PharmacySystem.Domain/PharmacySystemDomainModule.cs`

### Settings & Constants
- `src/PharmacySystem.Domain/PharmacySystemConsts.cs`
- `src/PharmacySystem.Domain/Settings/PharmacySystemSettings.cs`
- `src/PharmacySystem.Domain/Settings/PharmacySystemSettingDefinitionProvider.cs`
- `src/PharmacySystem.Domain/Identity/ChangeIdentityPasswordPolicySettingDefinitionProvider.cs`

### Migration Services
- `src/PharmacySystem.Domain/Data/IPharmacySystemDbSchemaMigrator.cs`
- `src/PharmacySystem.Domain/Data/NullPharmacySystemDbSchemaMigrator.cs`
- `src/PharmacySystem.Domain/Data/PharmacySystemDbMigrationService.cs`

### OpenIddict
- `src/PharmacySystem.Domain/OpenIddict/OpenIddictDataSeedContributor.cs`

---

## 🟠 DOMAIN SHARED LAYER - Constants & Localization

### Shared Configuration
- `src/PharmacySystem.Domain.Shared/PharmacySystemDomainSharedModule.cs`

### Localization
- `src/PharmacySystem.Domain.Shared/Localization/PharmacySystem/en.json` (English strings)

---

## 🟡 APPLICATION CONTRACTS - DTOs & Interfaces

### Permission Definitions
- `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissions.cs` ⭐ (Permission constants)
- `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissionDefinitionProvider.cs` (Register permissions)

### Category DTOs
- `src/PharmacySystem.Application.Contracts/Categories/ICategoryAppService.cs`
- `src/PharmacySystem.Application.Contracts/Categories/CategoryDto.cs`
- `src/PharmacySystem.Application.Contracts/Categories/CreateUpdateCategoryDto.cs`

### Medicine DTOs
- `src/PharmacySystem.Application.Contracts/Medicines/IMedicineAppService.cs`
- `src/PharmacySystem.Application.Contracts/Medicines/MedicineDto.cs`
- `src/PharmacySystem.Application.Contracts/Medicines/CreateUpdateMedicineDto.cs`
- `src/PharmacySystem.Application.Contracts/Medicines/CategoryLookupDto.cs` (Dropdown DTO)

### Supplier DTOs
- `src/PharmacySystem.Application.Contracts/Suppliers/ISupplierAppService.cs`
- `src/PharmacySystem.Application.Contracts/Suppliers/SupplierDto.cs`
- `src/PharmacySystem.Application.Contracts/Suppliers/CreateUpdateSupplierDto.cs`
- `src/PharmacySystem.Application.Contracts/Suppliers/SupplierLookupDto.cs` (Dropdown DTO)

### Customer DTOs
- `src/PharmacySystem.Application.Contracts/Customers/ICustomerAppService.cs`
- `src/PharmacySystem.Application.Contracts/Customers/CustomerDto.cs`
- `src/PharmacySystem.Application.Contracts/Customers/CreateUpdateCustomerDto.cs`
- `src/PharmacySystem.Application.Contracts/Customers/CustomerLookupDto.cs` (Dropdown DTO)

### Purchase DTOs ⭐
- `src/PharmacySystem.Application.Contracts/Purchases/IPurchaseAppService.cs` (Service interface)
- `src/PharmacySystem.Application.Contracts/Purchases/PurchaseDto.cs` ⭐ (Read DTO with items + concurrency stamp)
- `src/PharmacySystem.Application.Contracts/Purchases/CreateUpdatePurchaseDto.cs` ⭐ (Write DTO with items)
- `src/PharmacySystem.Application.Contracts/Purchases/PurchaseItemDto.cs` (Item DTO)
- `src/PharmacySystem.Application.Contracts/Purchases/CreateUpdatePurchaseItemDto.cs` (Item write DTO)
- `src/PharmacySystem.Application.Contracts/Purchases/MedicineLookupDto.cs` (Dropdown)
- `src/PharmacySystem.Application.Contracts/Purchases/SupplierLookupDto.cs` (Dropdown)

### Sale DTOs ⭐
- `src/PharmacySystem.Application.Contracts/Sales/ISaleAppService.cs`
- `src/PharmacySystem.Application.Contracts/Sales/SaleDto.cs` ⭐
- `src/PharmacySystem.Application.Contracts/Sales/CreateUpdateSaleDto.cs` ⭐
- `src/PharmacySystem.Application.Contracts/Sales/SaleItemDto.cs`
- `src/PharmacySystem.Application.Contracts/Sales/CreateUpdateSaleItemDto.cs`
- `src/PharmacySystem.Application.Contracts/Sales/MedicineLookupDto.cs`
- `src/PharmacySystem.Application.Contracts/Sales/CustomerLookupDto.cs`

### Stock DTOs ⭐
- `src/PharmacySystem.Application.Contracts/Stocks/IStockAppService.cs`
- `src/PharmacySystem.Application.Contracts/Stocks/StockDto.cs` ⭐
- `src/PharmacySystem.Application.Contracts/Stocks/LowStockDto.cs` ⭐ (Low-stock alerts)
- `src/PharmacySystem.Application.Contracts/Stocks/ExpiringStockDto.cs` (Expiry alerts)

### Dashboard DTOs ⭐
- `src/PharmacySystem.Application.Contracts/Dashboard/IDashboardAppService.cs`
- `src/PharmacySystem.Application.Contracts/Dashboard/DashboardStatsDto.cs` ⭐ (Main dashboard)
- `src/PharmacySystem.Application.Contracts/Dashboard/StockByCategoryDto.cs` (Stock by category)
- `src/PharmacySystem.Application.Contracts/Dashboard/ExpiryTimelineDto.cs` (Expiry timeline)
- `src/PharmacySystem.Application.Contracts/Dashboard/TopSellingMedicineDto.cs` (Top-selling)
- `src/PharmacySystem.Application.Contracts/Dashboard/SalesPurchasesTrendPointDto.cs` (Trends)

### Module & Extensions
- `src/PharmacySystem.Application.Contracts/PharmacySystemApplicationContractsModule.cs`
- `src/PharmacySystem.Application.Contracts/PharmacySystemDtoExtensions.cs` (DTO helper methods)

---

## 🟢 APPLICATION LAYER - Business Orchestration

### Application Services

#### Category Service
- `src/PharmacySystem.Application/Categories/CategoryAppService.cs`

#### Medicine Service
- `src/PharmacySystem.Application/Medicines/MedicineAppService.cs`

#### Supplier Service
- `src/PharmacySystem.Application/Suppliers/SupplierAppService.cs`

#### Customer Service
- `src/PharmacySystem.Application/Customers/CustomerAppService.cs`

#### Purchase Service ⭐
- `src/PharmacySystem.Application/Purchases/PurchaseAppService.cs` ⭐
  - CreateAsync: Save + IncreaseAsync stock
  - UpdateAsync: DecreaseAsync old + Update + IncreaseAsync new + Optimistic concurrency
  - DeleteAsync: DecreaseAsync + Delete
  - GetSupplierLookupAsync, GetMedicineLookupAsync
  - Fills SupplierName and MedicineName in DTOs

#### Sale Service ⭐
- `src/PharmacySystem.Application/Sales/SaleAppService.cs` ⭐
  - Similar pattern to Purchase but for sales

#### Stock Service ⭐
- `src/PharmacySystem.Application/Stocks/StockAppService.cs` ⭐
  - GetListAsync, GetAsync
  - GetLowStockAsync (filtered by minimum threshold)
  - GetExpiringStockAsync (near expiry)

#### Dashboard Service ⭐
- `src/PharmacySystem.Application/Dashboard/DashboardAppService.cs` ⭐
  - GetStatsAsync (overview)
  - GetStockByCategoryAsync (categorized stock)
  - GetExpiryTimelineAsync (expiry tracking)
  - GetTopSellingAsync (sales analytics)
  - GetLowStockAsync (alerts)
  - GetTrendAsync (historical trends)

### Base Service & Mapping
- `src/PharmacySystem.Application/PharmacySystemAppService.cs` (Base class for all services)
- `src/PharmacySystem.Application/PharmacySystemApplicationMappers.cs` ⭐ (Mapperly entity ↔ DTO)
- `src/PharmacySystem.Application/PharmacySystemApplicationModule.cs` (DI registration)

---

## 🔵 ENTITY FRAMEWORK CORE - Persistence

### DbContext & Factory
- `src/PharmacySystem.EntityFrameworkCore/EntityFrameworkCore/PharmacySystemDbContext.cs` ⭐
- `src/PharmacySystem.EntityFrameworkCore/EntityFrameworkCore/PharmacySystemDbContextFactory.cs` ⭐
- `src/PharmacySystem.EntityFrameworkCore/EntityFrameworkCore/PharmacySystemEFCoreEntityExtensions.cs`

### Entity Configurations
- `src/PharmacySystem.EntityFrameworkCore/Categories/CategoryConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Medicines/MedicineConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Suppliers/SupplierConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Customers/CustomerConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Purchases/PurchaseConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Purchases/PurchaseItemConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Sales/SaleConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Sales/SaleItemConfiguration.cs`
- `src/PharmacySystem.EntityFrameworkCore/Stocks/StockConfiguration.cs` ⭐

### Custom Repositories
- `src/PharmacySystem.EntityFrameworkCore/Sales/EfCoreSaleRepository.cs` (Implements ISaleRepository)

### Migrations ⭐
- `src/PharmacySystem.EntityFrameworkCore/Migrations/20260323055532_AddedSale.cs` (Sale + SaleItem tables)
- `src/PharmacySystem.EntityFrameworkCore/Migrations/20260323055532_AddedSale.Designer.cs`
- `src/PharmacySystem.EntityFrameworkCore/Migrations/20260323082404_AddedStock.cs` (Stock table with batch + expiry)
- `src/PharmacySystem.EntityFrameworkCore/Migrations/20260323082404_AddedStock.Designer.cs`
- `src/PharmacySystem.EntityFrameworkCore/Migrations/PharmacySystemDbContextModelSnapshot.cs` (Latest schema)

### Module
- `src/PharmacySystem.EntityFrameworkCore/PharmacySystemEntityFrameworkCoreModule.cs`

---

## 🔶 HTTP API - REST Endpoints

### Controllers
- `src/PharmacySystem.HttpApi/Controllers/CategoriesController.cs`
- `src/PharmacySystem.HttpApi/Controllers/MedicinesController.cs`
- `src/PharmacySystem.HttpApi/Controllers/SuppliersController.cs`
- `src/PharmacySystem.HttpApi/Controllers/CustomersController.cs`
- `src/PharmacySystem.HttpApi/Controllers/PurchasesController.cs` ⭐
- `src/PharmacySystem.HttpApi/Controllers/SalesController.cs` ⭐
- `src/PharmacySystem.HttpApi/Controllers/StocksController.cs` ⭐
- `src/PharmacySystem.HttpApi/Controllers/DashboardController.cs` ⭐

### Base Controller & Module
- `src/PharmacySystem.HttpApi/PharmacySystemController.cs` (Base routes)
- `src/PharmacySystem.HttpApi/PharmacySystemHttpApiModule.cs`

---

## 🟣 HTTP API HOST - Web Server & Entry Point

### Startup & Configuration ⭐
- `src/PharmacySystem.HttpApi.Host/Program.cs` ⭐ (Application entry point)
- `src/PharmacySystem.HttpApi.Host/PharmacySystemHttpApiHostModule.cs` ⭐ (Host module + DI)
- `src/PharmacySystem.HttpApi.Host/appsettings.json` ⭐ (Connection strings, logging)
- `src/PharmacySystem.HttpApi.Host/appsettings.secrets.json` (Sensitive config)

### Web Pages
- `src/PharmacySystem.HttpApi.Host/Pages/Index.cshtml`

### Properties & Docker
- `src/PharmacySystem.HttpApi.Host/Properties/launchSettings.json` (Debug settings)
- `src/PharmacySystem.HttpApi.Host/Dockerfile`
- `src/PharmacySystem.HttpApi.Host/.dockerignore`

---

## 🟤 DATABASE MIGRATOR - Migration Runner

### Entry Point & Configuration ⭐
- `src/PharmacySystem.DbMigrator/Program.cs` ⭐ (Migration execution)
- `src/PharmacySystem.DbMigrator/PharmacySystemDbMigratorModule.cs`
- `src/PharmacySystem.DbMigrator/appsettings.json` ⭐ (DB connection for migrations)
- `src/PharmacySystem.DbMigrator/appsettings.secrets.json`

---

## 🧪 TEST PROJECTS

### Test Base (Shared Infrastructure)
- `test/PharmacySystem.TestBase/PharmacySystemTestBase.cs` (Base test class)
- `test/PharmacySystem.TestBase/PharmacySystemTestBaseModule.cs` (Test DI)
- `test/PharmacySystem.TestBase/PharmacySystemTestConsts.cs`
- `test/PharmacySystem.TestBase/PharmacySystemTestDataBuilder.cs` (Test data helpers)
- `test/PharmacySystem.TestBase/Security/FakeCurrentPrincipalAccessor.cs` (Mock auth)
- `test/PharmacySystem.TestBase/appsettings.json` (Test DB config)

### Domain Tests
- `test/PharmacySystem.Domain.Tests/PharmacySystemDomainTestModule.cs`
- `test/PharmacySystem.Domain.Tests/Categories/CategoryTests.cs`
- `test/PharmacySystem.Domain.Tests/Medicines/MedicineTests.cs`
- `test/PharmacySystem.Domain.Tests/Purchases/PurchaseTests.cs` ⭐
- `test/PharmacySystem.Domain.Tests/Sales/SaleTests.cs`
- `test/PharmacySystem.Domain.Tests/Stocks/StockManagerTests.cs` ⭐

### Application Tests
- `test/PharmacySystem.Application.Tests/PharmacySystemApplicationTestsModule.cs`
- `test/PharmacySystem.Application.Tests/Categories/CategoryAppServiceTests.cs`
- `test/PharmacySystem.Application.Tests/Medicines/MedicineAppServiceTests.cs`
- `test/PharmacySystem.Application.Tests/Purchases/PurchaseAppServiceTests.cs` ⭐
- `test/PharmacySystem.Application.Tests/Sales/SaleAppServiceTests.cs`
- `test/PharmacySystem.Application.Tests/Stocks/StockAppServiceTests.cs`
- `test/PharmacySystem.Application.Tests/Dashboard/DashboardAppServiceTests.cs` ⭐

### EF Core Tests
- `test/PharmacySystem.EntityFrameworkCore.Tests/PharmacySystemEntityFrameworkCoreTestsModule.cs`
- `test/PharmacySystem.EntityFrameworkCore.Tests/Categories/CategoryRepositoryTests.cs`
- `test/PharmacySystem.EntityFrameworkCore.Tests/Purchases/PurchaseRepositoryTests.cs`
- `test/PharmacySystem.EntityFrameworkCore.Tests/Sales/SaleRepositoryTests.cs`
- `test/PharmacySystem.EntityFrameworkCore.Tests/Stocks/StockRepositoryTests.cs`

### API Client Console Test
- `test/PharmacySystem.HttpApi.Client.ConsoleTestApp/Program.cs` ⭐ (Console entry)
- `test/PharmacySystem.HttpApi.Client.ConsoleTestApp/PharmacySystemConsoleApiClientModule.cs` (Client DI)
- `test/PharmacySystem.HttpApi.Client.ConsoleTestApp/ClientDemoService.cs` (Example API calls)
- `test/PharmacySystem.HttpApi.Client.ConsoleTestApp/appsettings.json` (Host URL)

---

## Client Libraries

### HTTP API Client
- `src/PharmacySystem.HttpApi.Client/PharmacySystem.HttpApi.Client.csproj`
- `src/PharmacySystem.HttpApi.Client/PharmacySystemHttpApiClientModule.cs` (Client module)

---

## Quick Start Commands

```powershell
# Navigate to repo
cd E:\PesonalProjects\PharmacySystem

# Restore all packages
dotnet restore

# Build entire solution
dotnet build

# Run database migrations
dotnet run --project src/PharmacySystem.DbMigrator

# Start the API server
dotnet run --project src/PharmacySystem.HttpApi.Host
# API will be available at: https://localhost:44379/swagger or http://localhost:5192/swagger

# Run all tests
dotnet test

# Run specific test project
dotnet test test/PharmacySystem.Application.Tests

# Run console test app
dotnet run --project test/PharmacySystem.HttpApi.Client.ConsoleTestApp
```

---

## Configuration Files

### Connection Strings & Logging
- `src/PharmacySystem.HttpApi.Host/appsettings.json` (Main host config)
- `src/PharmacySystem.DbMigrator/appsettings.json` (Migration config)
- `test/PharmacySystem.TestBase/appsettings.json` (Test DB config)

### Git Configuration
- `.gitignore` (Ignore patterns)

### Code Style & Standards
- `.editorconfig` (Code style rules)

---

## Critical ⭐ Files (Must-Read for Development)

1. **Stock Management Logic:**
   - `src/PharmacySystem.Domain/Stocks/StockManager.cs`
   - `src/PharmacySystem.Application/Purchases/PurchaseAppService.cs`

2. **Data Models:**
   - `src/PharmacySystem.Domain/Purchases/Purchase.cs`
   - `src/PharmacySystem.Domain/Stocks/Stock.cs`
   - `src/PharmacySystem.EntityFrameworkCore/EntityFrameworkCore/PharmacySystemDbContext.cs`

3. **DTOs & Contracts:**
   - `src/PharmacySystem.Application.Contracts/Purchases/PurchaseDto.cs`
   - `src/PharmacySystem.Application.Contracts/Purchases/CreateUpdatePurchaseDto.cs`

4. **Permissions:**
   - `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissions.cs`

5. **API Startup:**
   - `src/PharmacySystem.HttpApi.Host/Program.cs`
   - `src/PharmacySystem.HttpApi.Host/appsettings.json`

6. **Mappings:**
   - `src/PharmacySystem.Application/PharmacySystemApplicationMappers.cs`

7. **Database:**
   - `src/PharmacySystem.EntityFrameworkCore/Migrations/20260323082404_AddedStock.cs`

---

This list can be copied and pasted directly for team reference or external documentation!
