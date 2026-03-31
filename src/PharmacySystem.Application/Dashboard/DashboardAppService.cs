using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
using PharmacySystem.Purchases;
using PharmacySystem.Sales;
using PharmacySystem.Stocks;
using PharmacySystem.Suppliers;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using PharmacySystem.Categories;

namespace PharmacySystem.Dashboard;

// Application service for dashboard statistics
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Purchase, Guid> _purchaseRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IRepository<Stock, Guid> _stockRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public DashboardAppService(
        IRepository<Medicine, Guid> medicineRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Purchase, Guid> purchaseRepository,
        ISaleRepository saleRepository,
        IRepository<Stock, Guid> stockRepository,
        IRepository<Category, Guid> categoryRepository)
    {
        _medicineRepository = medicineRepository;
        _supplierRepository = supplierRepository;
        _customerRepository = customerRepository;
        _purchaseRepository = purchaseRepository;
        _saleRepository = saleRepository;
        _stockRepository = stockRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalMedicines = await _medicineRepository.GetCountAsync();
        var totalSuppliers = await _supplierRepository.GetCountAsync();
        var totalCustomers = await _customerRepository.GetCountAsync();
        var totalPurchases = await _purchaseRepository.GetCountAsync();
        var totalSales = await _saleRepository.GetCountAsync();
        var totalStockRows = await _stockRepository.GetCountAsync();

        var stocks = await _stockRepository.GetListAsync();
        var medicines = await _medicineRepository.GetListAsync();

        var lowStockCount = stocks
            .Join(
                medicines,
                stock => stock.MedicineId,
                medicine => medicine.Id,
                (stock, medicine) => new
                {
                    stock.Quantity,
                    medicine.ReorderLevel
                }
            )
            .Count(x => x.Quantity <= x.ReorderLevel);

        var today = DateTime.Today;

        var expiringStockCount = stocks.Count(x =>
            x.ExpiryDate.HasValue &&
            (x.ExpiryDate.Value.Date < today || (x.ExpiryDate.Value.Date - today).Days <= 30)
        );

        return new DashboardStatsDto
        {
            TotalMedicines = totalMedicines,
            TotalSuppliers = totalSuppliers,
            TotalCustomers = totalCustomers,
            TotalPurchases = totalPurchases,
            TotalSales = totalSales,
            TotalStockRows = totalStockRows,
            LowStockCount = lowStockCount,
            ExpiringStockCount = expiringStockCount
        };
    }

    public async Task<List<SalesPurchasesTrendPointDto>> GetSalesPurchasesTrendAsync()
    {
        var today = DateTime.Today;
        var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

        var purchases = await _purchaseRepository.GetListAsync(x => x.PurchaseDate >= startMonth);
        var sales = await _saleRepository.GetListAsync(x => x.SaleDate >= startMonth);

        var purchaseLookup = purchases
            .GroupBy(x => new { x.PurchaseDate.Year, x.PurchaseDate.Month })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month}",
                g => g.Sum(x => x.NetAmount)
            );

        var salesLookup = sales
            .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month}",
                g => g.Sum(x => x.NetAmount)
            );

        var result = new List<SalesPurchasesTrendPointDto>();

        for (var i = 0; i < 6; i++)
        {
            var month = startMonth.AddMonths(i);
            var key = $"{month.Year}-{month.Month}";

            result.Add(new SalesPurchasesTrendPointDto
            {
                Label = month.ToString("MMM yyyy"),
                PurchaseAmount = purchaseLookup.GetValueOrDefault(key, 0),
                SalesAmount = salesLookup.GetValueOrDefault(key, 0)
            });
        }

        return result;
    }

    public async Task<List<TopSellingMedicineDto>> GetTopSellingMedicinesAsync()
    {
        var topSelling = await _saleRepository.GetTopSellingMedicinesAsync(5);
        var medicines = await _medicineRepository.GetListAsync();

        var result = topSelling
            .Join(
                medicines,
                top => top.MedicineId,
                medicine => medicine.Id,
                (top, medicine) => new TopSellingMedicineDto
                {
                    MedicineName = medicine.Name,
                    TotalQuantitySold = top.TotalQuantitySold
                }
            )
            .ToList();

        return result;
    }

    public async Task<List<StockByCategoryDto>> GetStockByCategoryAsync()
    {
        var stocks = await _stockRepository.GetListAsync();
        var medicines = await _medicineRepository.GetListAsync();
        var categories = await _categoryRepository.GetListAsync();

        var result = stocks
            .Join(
                medicines,
                stock => stock.MedicineId,
                medicine => medicine.Id,
                (stock, medicine) => new
                {
                    stock.Quantity,
                    medicine.CategoryId
                }
            )
            .Join(
                categories,
                sm => sm.CategoryId,
                category => category.Id,
                (sm, category) => new
                {
                    category.Name,
                    sm.Quantity
                }
            )
            .GroupBy(x => x.Name)
            .Select(g => new StockByCategoryDto
            {
                CategoryName = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ToList();

        return result;
    }

    public async Task<List<ExpiryTimelineDto>> GetExpiryTimelineAsync()
    {
        var today = DateTime.Today;
        var stocks = await _stockRepository.GetListAsync();

        var expiredQuantity = stocks
            .Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value.Date < today)
            .Sum(x => x.Quantity);

        var zeroTo30Quantity = stocks
            .Where(x =>
                x.ExpiryDate.HasValue &&
                x.ExpiryDate.Value.Date >= today &&
                (x.ExpiryDate.Value.Date - today).Days <= 30)
            .Sum(x => x.Quantity);

        var thirtyOneTo60Quantity = stocks
            .Where(x =>
                x.ExpiryDate.HasValue &&
                (x.ExpiryDate.Value.Date - today).Days >= 31 &&
                (x.ExpiryDate.Value.Date - today).Days <= 60)
            .Sum(x => x.Quantity);

        var sixtyOneTo90Quantity = stocks
            .Where(x =>
                x.ExpiryDate.HasValue &&
                (x.ExpiryDate.Value.Date - today).Days >= 61 &&
                (x.ExpiryDate.Value.Date - today).Days <= 90)
            .Sum(x => x.Quantity);

        var over90Quantity = stocks
            .Where(x =>
                x.ExpiryDate.HasValue &&
                (x.ExpiryDate.Value.Date - today).Days > 90)
            .Sum(x => x.Quantity);

        return new List<ExpiryTimelineDto>
    {
        new ExpiryTimelineDto
        {
            Label = "Expired",
            TotalQuantity = expiredQuantity
        },
        new ExpiryTimelineDto
        {
            Label = "0-30 Days",
            TotalQuantity = zeroTo30Quantity
        },
        new ExpiryTimelineDto
        {
            Label = "31-60 Days",
            TotalQuantity = thirtyOneTo60Quantity
        },
        new ExpiryTimelineDto
        {
            Label = "61-90 Days",
            TotalQuantity = sixtyOneTo90Quantity
        },
        new ExpiryTimelineDto
        {
            Label = "90+ Days",
            TotalQuantity = over90Quantity
        }
    };
    }
}