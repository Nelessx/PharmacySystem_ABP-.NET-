# PharmacySystem - Complete Folder Structure

## Root Level
```
E:\PesonalProjects\PharmacySystem\
├── PharmacySystem.sln                    # Solution file
├── global.json                            # Global configuration
├── .gitignore                             # Git ignore rules
├── .editorconfig                          # Editor configuration
├── README.md                              # Project documentation
└── Directory.Build.props                  # Build properties
```

---

## src/ - Source Code

### src/PharmacySystem.Domain (Domain Layer)
```
src/PharmacySystem.Domain/
├── PharmacySystem.Domain.csproj
├── PharmacySystemDomainModule.cs          # Domain module registration
├── PharmacySystemConsts.cs                # Domain constants
│
├── Categories/
│   └── Category.cs                        # Category entity
│
├── Medicines/
│   └── Medicine.cs                        # Medicine entity (with category reference)
│
├── Suppliers/
│   └── Supplier.cs                        # Supplier entity
│
├── Customers/
│   └── Customer.cs                        # Customer entity
│
├── Purchases/
│   ├── Purchase.cs                        # Purchase aggregate root (header)
│   ├── PurchaseItem.cs                    # Purchase line items
│
├── Sales/
│   ├── Sale.cs                            # Sale aggregate root (header)
│   ├── SaleItem.cs                        # Sale line items
│   ├── ISaleRepository.cs                 # Sale repository interface (custom domain repository)
│
├── Stocks/
│   ├── Stock.cs                           # Stock entity (batch + expiry-aware)
│   ├── StockManager.cs                    # Domain service for stock operations
│
├── Settings/
│   ├── PharmacySystemSettings.cs          # Settings constants
│   └── PharmacySystemSettingDefinitionProvider.cs  # Setting definitions
│
├── Identity/
│   └── ChangeIdentityPasswordPolicySettingDefinitionProvider.cs
│
├── OpenIddict/
│   └── OpenIddictDataSeedContributor.cs
│
└── Data/
    ├── IPharmacySystemDbSchemaMigrator.cs
    ├── NullPharmacySystemDbSchemaMigrator.cs
    └── PharmacySystemDbMigrationService.cs
```

### src/PharmacySystem.Domain.Shared (Shared Constants)
```
src/PharmacySystem.Domain.Shared/
├── PharmacySystem.Domain.Shared.csproj
├── PharmacySystemDomainSharedModule.cs
│
└── Localization/
    └── PharmacySystem/
        └── en.json                        # English localization strings
```

### src/PharmacySystem.Application.Contracts (Contracts/DTOs)
```
src/PharmacySystem.Application.Contracts/
├── PharmacySystem.Application.Contracts.csproj
├── PharmacySystemApplicationContractsModule.cs
│
├── Permissions/
│   ├── PharmacySystemPermissions.cs       # Permission constant definitions
│   └── PharmacySystemPermissionDefinitionProvider.cs  # Permission registrations
│
├── Categories/
│   ├── ICategoryAppService.cs             # Service interface
│   ├── CategoryDto.cs                     # Read DTO
│   └── CreateUpdateCategoryDto.cs         # Write DTO
│
├── Medicines/
│   ├── IMedicineAppService.cs
│   ├── MedicineDto.cs
│   ├── CreateUpdateMedicineDto.cs
│   └── CategoryLookupDto.cs               # Lightweight DTO for dropdowns
│
├── Suppliers/
│   ├── ISupplierAppService.cs
│   ├── SupplierDto.cs
│   ├── CreateUpdateSupplierDto.cs
│   └── SupplierLookupDto.cs
│
├── Customers/
│   ├── ICustomerAppService.cs
│   ├── CustomerDto.cs
│   ├── CreateUpdateCustomerDto.cs
│   └── CustomerLookupDto.cs
│
├── Purchases/
│   ├── IPurchaseAppService.cs             # Purchase service interface
│   ├── PurchaseDto.cs                     # Purchase read DTO (with items)
│   ├── CreateUpdatePurchaseDto.cs         # Purchase write DTO (with items)
│   ├── PurchaseItemDto.cs                 # Purchase line item DTO
│   ├── CreateUpdatePurchaseItemDto.cs     # Purchase item write DTO
│   ├── MedicineLookupDto.cs               # Lookup DTO for dropdowns
│   └── SupplierLookupDto.cs               # Lookup DTO for dropdowns
│
├── Sales/
│   ├── ISaleAppService.cs
│   ├── SaleDto.cs
│   ├── CreateUpdateSaleDto.cs
│   ├── SaleItemDto.cs
│   ├── CreateUpdateSaleItemDto.cs
│   ├── MedicineLookupDto.cs
│   └── CustomerLookupDto.cs
│
├── Stocks/
│   ├── IStockAppService.cs
│   ├── StockDto.cs
│   ├── LowStockDto.cs                     # DTO for low-stock alerts
│   └── ExpiringStockDto.cs                # DTO for expiring stock alerts
│
├── Dashboard/
│   ├── IDashboardAppService.cs
│   ├── DashboardStatsDto.cs               # Main dashboard stats DTO
│   ├── StockByCategoryDto.cs              # Stock grouped by category
│   ├── ExpiryTimelineDto.cs               # Expiry timeline data
│   ├── TopSellingMedicineDto.cs           # Top-selling medicines
│   ├── SalesPurchasesTrendPointDto.cs     # Trend point for charting
│   └── LowStockDto.cs
│
└── PharmacySystemDtoExtensions.cs         # DTO helper methods
```

### src/PharmacySystem.Application (Application Layer / Services)
```
src/PharmacySystem.Application/
├── PharmacySystem.Application.csproj
├── PharmacySystemApplicationModule.cs     # Module registration + dependency injection
├── PharmacySystemAppService.cs            # Base app service with common functionality
│
├── Categories/
│   └── CategoryAppService.cs              # Category CRUD service
│
├── Medicines/
│   └── MedicineAppService.cs              # Medicine CRUD service
│
├── Suppliers/
│   └── SupplierAppService.cs              # Supplier CRUD service
│
├── Customers/
│   └── CustomerAppService.cs              # Customer CRUD service
│
├── Purchases/
│   └── PurchaseAppService.cs              # Purchase CRUD + stock management
│                                           # - CreateAsync: save + increase stock
│                                           # - UpdateAsync: reverse old stock + update + apply new stock
│                                           # - DeleteAsync: reverse stock + delete
│                                           # - GetSupplierLookupAsync, GetMedicineLookupAsync
│
├── Sales/
│   └── SaleAppService.cs                  # Sale CRUD + stock management
│
├── Stocks/
│   └── StockAppService.cs                 # Stock CRUD + low-stock/expiry queries
│
├── Dashboard/
│   └── DashboardAppService.cs             # Analytics + reporting endpoints
│                                           # - GetStatsAsync, GetStockByCategoryAsync
│                                           # - GetExpiryTimelineAsync, GetTopSellingAsync
│                                           # - GetLowStockAsync, GetTrendAsync
│
└── PharmacySystemApplicationMappers.cs    # Mapperly configuration for DTO ↔ Entity
```

### src/PharmacySystem.EntityFrameworkCore (Persistence/EF Core)
```
src/PharmacySystem.EntityFrameworkCore/
├── PharmacySystem.EntityFrameworkCore.csproj
├── PharmacySystemEntityFrameworkCoreModule.cs
│
├── EntityFrameworkCore/
│   ├── PharmacySystemDbContext.cs         # Main DbContext with all DbSets
│   ├── PharmacySystemDbContextFactory.cs  # Design-time factory for migrations
│   └── PharmacySystemEFCoreEntityExtensions.cs
│
├── Categories/
│   └── CategoryConfiguration.cs            # EF Core entity mapping configuration
│
├── Medicines/
│   └── MedicineConfiguration.cs
│
├── Suppliers/
│   └── SupplierConfiguration.cs
│
├── Customers/
│   └── CustomerConfiguration.cs
│
├── Purchases/
│   ├── PurchaseConfiguration.cs
│   └── PurchaseItemConfiguration.cs
│
├── Sales/
│   ├── SaleConfiguration.cs
│   ├── SaleItemConfiguration.cs
│   └── EfCoreSaleRepository.cs            # Custom repository implementation for ISaleRepository
│
├── Stocks/
│   └── StockConfiguration.cs
│
└── Migrations/
    ├── 20260323055532_AddedSale.cs        # Migration: Added Sale and SaleItem tables
    ├── 20260323055532_AddedSale.Designer.cs
    ├── 20260323082404_AddedStock.cs       # Migration: Added Stock table
    ├── 20260323082404_AddedStock.Designer.cs
    ├── 20260324000000_Initial.cs          # Initial schema
    └── PharmacySystemDbContextModelSnapshot.cs  # Latest schema snapshot
```

### src/PharmacySystem.HttpApi (HTTP API Routes)
```
src/PharmacySystem.HttpApi/
├── PharmacySystem.HttpApi.csproj
├── PharmacySystemHttpApiModule.cs
│
├── Controllers/
│   ├── CategoriesController.cs            # REST endpoints for categories
│   ├── MedicinesController.cs             # REST endpoints for medicines
│   ├── SuppliersController.cs             # REST endpoints for suppliers
│   ├── CustomersController.cs             # REST endpoints for customers
│   ├── PurchasesController.cs             # REST endpoints for purchases
│   ├── SalesController.cs                 # REST endpoints for sales
│   ├── StocksController.cs                # REST endpoints for stocks
│   └── DashboardController.cs             # REST endpoints for dashboard
│
└── PharmacySystemController.cs            # Base controller with common routes
```

### src/PharmacySystem.HttpApi.Host (API Host / Entry Point)
```
src/PharmacySystem.HttpApi.Host/
├── PharmacySystem.HttpApi.Host.csproj
├── Program.cs                             # Application startup & configuration
├── PharmacySystemHttpApiHostModule.cs     # Host module registration
├── appsettings.json                       # Configuration: connection strings, logging
├── appsettings.secrets.json               # Sensitive configuration (local only)
├── .dockerignore
├── Dockerfile
│
├── Pages/
│   └── Index.cshtml                       # Default page
│
├── Properties/
│   └── launchSettings.json                # Debug launch settings
│
└── wwwroot/                               # Static files
```

### src/PharmacySystem.DbMigrator (Migration Runner)
```
src/PharmacySystem.DbMigrator/
├── PharmacySystem.DbMigrator.csproj
├── Program.cs                             # Migration execution entry point
├── PharmacySystemDbMigratorModule.cs
├── appsettings.json                       # DB connection config for migrations
└── appsettings.secrets.json
```

---

## test/ - Test Projects

### test/PharmacySystem.TestBase (Shared Test Infrastructure)
```
test/PharmacySystem.TestBase/
├── PharmacySystem.TestBase.csproj
├── PharmacySystemTestBase.cs              # Base test class with test app factory
├── PharmacySystemTestBaseModule.cs        # Test module registration
├── PharmacySystemTestConsts.cs            # Test constants
├── PharmacySystemTestDataBuilder.cs       # Helper for test data creation
│
├── Security/
│   └── FakeCurrentPrincipalAccessor.cs    # Mock principal for auth testing
│
└── appsettings.json                       # Test DB configuration (in-memory or test DB)
```

### test/PharmacySystem.Domain.Tests (Domain Layer Tests)
```
test/PharmacySystem.Domain.Tests/
├── PharmacySystem.Domain.Tests.csproj
├── PharmacySystemDomainTestModule.cs
│
├── Categories/
│   └── CategoryTests.cs
│
├── Medicines/
│   └── MedicineTests.cs
│
├── Purchases/
│   └── PurchaseTests.cs                   # Tests for Purchase aggregate
│
├── Sales/
│   └── SaleTests.cs
│
├── Stocks/
│   └── StockManagerTests.cs               # Tests for stock operations
│
└── appsettings.json
```

### test/PharmacySystem.Application.Tests (Application Layer Tests)
```
test/PharmacySystem.Application.Tests/
├── PharmacySystem.Application.Tests.csproj
├── PharmacySystemApplicationTestsModule.cs
│
├── Categories/
│   └── CategoryAppServiceTests.cs
│
├── Medicines/
│   └── MedicineAppServiceTests.cs
│
├── Purchases/
│   └── PurchaseAppServiceTests.cs         # Tests for Purchase service
│                                           # - Create, Update, Delete operations
│                                           # - Stock reconciliation
│                                           # - Concurrency handling
│
├── Sales/
│   └── SaleAppServiceTests.cs
│
├── Stocks/
│   └── StockAppServiceTests.cs
│
├── Dashboard/
│   └── DashboardAppServiceTests.cs        # Tests for analytics endpoints
│
└── appsettings.json
```

### test/PharmacySystem.EntityFrameworkCore.Tests (Persistence Layer Tests)
```
test/PharmacySystem.EntityFrameworkCore.Tests/
├── PharmacySystem.EntityFrameworkCore.Tests.csproj
├── PharmacySystemEntityFrameworkCoreTestsModule.cs
│
├── Categories/
│   └── CategoryRepositoryTests.cs
│
├── Purchases/
│   └── PurchaseRepositoryTests.cs
│
├── Sales/
│   └── SaleRepositoryTests.cs
│
├── Stocks/
│   └── StockRepositoryTests.cs
│
└── appsettings.json
```

### test/PharmacySystem.HttpApi.Client.ConsoleTestApp (Client Test Console)
```
test/PharmacySystem.HttpApi.Client.ConsoleTestApp/
├── PharmacySystem.HttpApi.Client.ConsoleTestApp.csproj
├── Program.cs                             # Console entry point
├── PharmacySystemConsoleApiClientModule.cs
├── ClientDemoService.cs                   # Example API calls
├── appsettings.json                       # Host URL configuration
└── appsettings.secrets.json
```

---

## Key Important Files Quick Reference

### 🔷 Core Business Logic
- `src/PharmacySystem.Application/Purchases/PurchaseAppService.cs` — Stock reversal + update logic
- `src/PharmacySystem.Application/Sales/SaleAppService.cs` — Sales with stock operations
- `src/PharmacySystem.Domain/Stocks/StockManager.cs` — Domain service for stock increase/decrease
- `src/PharmacySystem.Application/Dashboard/DashboardAppService.cs` — Analytics endpoints

### 🔷 Domain Entities
- `src/PharmacySystem.Domain/Purchases/Purchase.cs` — Purchase aggregate (batch + expiry support)
- `src/PharmacySystem.Domain/Sales/Sale.cs` — Sale aggregate
- `src/PharmacySystem.Domain/Stocks/Stock.cs` — Stock with batch tracking
- `src/PharmacySystem.Domain/Medicines/Medicine.cs` — Medicine entity

### 🔷 DTOs & Contracts
- `src/PharmacySystem.Application.Contracts/Purchases/CreateUpdatePurchaseDto.cs`
- `src/PharmacySystem.Application.Contracts/Purchases/PurchaseDto.cs`
- `src/PharmacySystem.Application.Contracts/Dashboard/DashboardStatsDto.cs`
- `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissions.cs`

### 🔷 Permissions & Security
- `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissionDefinitionProvider.cs`
- `src/PharmacySystem.Application.Contracts/Permissions/PharmacySystemPermissions.cs`

### 🔷 Database & Persistence
- `src/PharmacySystem.EntityFrameworkCore/EntityFrameworkCore/PharmacySystemDbContext.cs`
- `src/PharmacySystem.EntityFrameworkCore/Migrations/` — All migrations
- `src/PharmacySystem.EntityFrameworkCore/Stocks/StockConfiguration.cs`

### 🔷 Configuration & Mapping
- `src/PharmacySystem.Application/PharmacySystemApplicationMappers.cs` — Mapperly setup
- `src/PharmacySystem.HttpApi.Host/appsettings.json` — Connection strings, logging
- `src/PharmacySystem.DbMigrator/appsettings.json` — Migration DB config

### 🔷 Module & DI Setup
- `src/PharmacySystem.Application/PharmacySystemApplicationModule.cs`
- `src/PharmacySystem.HttpApi.Host/PharmacySystemHttpApiHostModule.cs`
- `src/PharmacySystem.EntityFrameworkCore/PharmacySystemEntityFrameworkCoreModule.cs`

### 🔷 Entry Points
- `src/PharmacySystem.HttpApi.Host/Program.cs` — API host startup
- `src/PharmacySystem.DbMigrator/Program.cs` — Migration runner startup
- `test/PharmacySystem.HttpApi.Client.ConsoleTestApp/Program.cs` — Console client test

---

## File Statistics

| Category | Count | Key Files |
|----------|-------|-----------|
| Application Services | 8 | Purchase, Sale, Stock, Dashboard, Category, Medicine, Supplier, Customer |
| Domain Entities | 8 | Purchase, PurchaseItem, Sale, SaleItem, Stock, Medicine, Supplier, Customer, Category |
| DTOs (Contracts) | 30+ | Purchase, Sale, Stock, Dashboard, Medicines, Suppliers, Customers |
| Configurations | 8+ | Entity mappings for all major entities |
| Migrations | 2 | Sale, Stock |
| Tests | 4 projects | Domain, Application, EF Core, Integration |
| Controllers/Routes | 8 | REST endpoints for all modules |

---

## Build & Run Commands

```powershell
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run migrations
dotnet run --project src/PharmacySystem.DbMigrator

# Start API Host
dotnet run --project src/PharmacySystem.HttpApi.Host

# Run tests
dotnet test

# Run console client test
dotnet run --project test/PharmacySystem.HttpApi.Client.ConsoleTestApp
```

---

## Architecture Layers

```
┌─────────────────────────────────────┐
│  HttpApi.Host / Razor Pages (UI)    │  ← Presentation
├─────────────────────────────────────┤
│  HttpApi (Controllers)              │  ← API Routes
├─────────────────────────────────────┤
│  Application (Services)             │  ← Business Orchestration
│  - CRUD operations                  │
│  - Stock reconciliation             │
│  - Dashboard/Analytics              │
├─────────────────────────────────────┤
│  Domain (Entities + Domain Services)│  ← Business Rules
│  - Purchase, Sale, Stock Aggregates │
│  - StockManager domain service      │
├─────────────────────────────────────┤
│  EntityFrameworkCore (Persistence)  │  ← Data Access
│  - DbContext, Migrations, Configs   │
└─────────────────────────────────────┘
```

---

This structure can be directly copied and pasted for team documentation or external sharing!
