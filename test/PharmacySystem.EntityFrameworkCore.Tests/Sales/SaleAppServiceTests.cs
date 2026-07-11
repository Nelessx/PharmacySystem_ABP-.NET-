using System;
using System.Threading.Tasks;
using PharmacySystem.Categories;
using PharmacySystem.Medicines;
using PharmacySystem.Sales;
using PharmacySystem.Stocks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PharmacySystem.EntityFrameworkCore.Sales;

[Collection(PharmacySystemTestConsts.CollectionDefinitionName)]
public class SaleAppServiceTests : PharmacySystemEntityFrameworkCoreTestBase
{
    private readonly ISaleAppService _saleAppService;
    private readonly StockManager _stockManager;
    private readonly IRepository<Stock, Guid> _stockRepository;
    private readonly IRepository<Sale, Guid> _saleRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public SaleAppServiceTests()
    {
        _saleAppService = GetRequiredService<ISaleAppService>();
        _stockManager = GetRequiredService<StockManager>();
        _stockRepository = GetRequiredService<IRepository<Stock, Guid>>();
        _saleRepository = GetRequiredService<IRepository<Sale, Guid>>();
        _medicineRepository = GetRequiredService<IRepository<Medicine, Guid>>();
        _categoryRepository = GetRequiredService<IRepository<Category, Guid>>();
    }

    // Creates a persisted medicine with a single stock lot and returns its id.
    private async Task<Guid> SeedMedicineWithStockAsync(string batch, DateTime expiry, int quantity, decimal cost)
    {
        var categoryId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await _categoryRepository.InsertAsync(
                new Category(categoryId, "Cat-" + medicineId.ToString("N")[..6], null, true), autoSave: true);
            await _medicineRepository.InsertAsync(
                new Medicine(medicineId, "Med-" + medicineId.ToString("N")[..6], categoryId, cost, cost + 5m), autoSave: true);
            await _stockManager.IncreaseAsync(medicineId, batch, expiry, quantity, cost);
        });

        return medicineId;
    }

    [Fact]
    public async Task CreateAsync_decrements_stock_for_each_item()
    {
        var expiry = DateTime.Today.AddYears(1);
        var medicineId = await SeedMedicineWithStockAsync("B1", expiry, 100, 10m);

        await _saleAppService.CreateAsync(new CreateUpdateSaleDto
        {
            SaleNumber = "SAL-DEC-1",
            SaleDate = DateTime.Today,
            Items =
            {
                new CreateUpdateSaleItemDto
                {
                    MedicineId = medicineId, BatchNumber = "B1", ExpiryDate = expiry, Quantity = 10, UnitPrice = 15m
                }
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var stock = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            stock!.Quantity.ShouldBe(90);
        });
    }

    [Fact]
    public async Task DeleteAsync_restores_stock_without_corrupting_unit_cost()
    {
        var expiry = DateTime.Today.AddYears(1);
        var medicineId = await SeedMedicineWithStockAsync("B1", expiry, 100, 10m);

        var sale = await _saleAppService.CreateAsync(new CreateUpdateSaleDto
        {
            SaleNumber = "SAL-DEL-1",
            SaleDate = DateTime.Today,
            Items =
            {
                // Note: UnitPrice (retail) 15 differs from the batch cost 10.
                new CreateUpdateSaleItemDto
                {
                    MedicineId = medicineId, BatchNumber = "B1", ExpiryDate = expiry, Quantity = 10, UnitPrice = 15m
                }
            }
        });

        await _saleAppService.DeleteAsync(sale.Id);

        await WithUnitOfWorkAsync(async () =>
        {
            var stock = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            stock!.Quantity.ShouldBe(100);   // fully restored
            stock.UnitCost.ShouldBe(10m);    // cost preserved, NOT overwritten with the 15 sale price
        });
    }

    [Fact]
    public async Task CreateAsync_with_insufficient_stock_throws()
    {
        var expiry = DateTime.Today.AddYears(1);
        var medicineId = await SeedMedicineWithStockAsync("B1", expiry, 5, 10m);

        await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(new CreateUpdateSaleDto
        {
            SaleNumber = "SAL-INSUF-1",
            SaleDate = DateTime.Today,
            Items =
            {
                new CreateUpdateSaleItemDto
                {
                    MedicineId = medicineId, BatchNumber = "B1", ExpiryDate = expiry, Quantity = 50, UnitPrice = 15m
                }
            }
        }));
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_sale_number()
    {
        var expiry = DateTime.Today.AddYears(1);
        var medicineId = await SeedMedicineWithStockAsync("B1", expiry, 100, 10m);

        CreateUpdateSaleDto Build() => new()
        {
            SaleNumber = "SAL-DUP-1",
            SaleDate = DateTime.Today,
            Items =
            {
                new CreateUpdateSaleItemDto
                {
                    MedicineId = medicineId, BatchNumber = "B1", ExpiryDate = expiry, Quantity = 1, UnitPrice = 15m
                }
            }
        };

        await _saleAppService.CreateAsync(Build());
        await Should.ThrowAsync<UserFriendlyException>(() => _saleAppService.CreateAsync(Build()));
    }

    [Fact]
    public async Task CreateAsync_rejects_discount_greater_than_total()
    {
        var expiry = DateTime.Today.AddYears(1);
        var medicineId = await SeedMedicineWithStockAsync("B1", expiry, 100, 10m);

        await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(new CreateUpdateSaleDto
        {
            SaleNumber = "SAL-DISC-1",
            SaleDate = DateTime.Today,
            DiscountAmount = 1000m,
            Items =
            {
                new CreateUpdateSaleItemDto
                {
                    MedicineId = medicineId, BatchNumber = "B1", ExpiryDate = expiry, Quantity = 1, UnitPrice = 15m
                }
            }
        }));
    }
}
