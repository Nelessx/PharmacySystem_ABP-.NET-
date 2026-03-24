using System;
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

namespace PharmacySystem.Dashboard;

// Application service for dashboard statistics
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Purchase, Guid> _purchaseRepository;
    private readonly IRepository<Sale, Guid> _saleRepository;
    private readonly IRepository<Stock, Guid> _stockRepository;

    public DashboardAppService(
        IRepository<Medicine, Guid> medicineRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Purchase, Guid> purchaseRepository,
        IRepository<Sale, Guid> saleRepository,
        IRepository<Stock, Guid> stockRepository)
    {
        _medicineRepository = medicineRepository;
        _supplierRepository = supplierRepository;
        _customerRepository = customerRepository;
        _purchaseRepository = purchaseRepository;
        _saleRepository = saleRepository;
        _stockRepository = stockRepository;
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
}