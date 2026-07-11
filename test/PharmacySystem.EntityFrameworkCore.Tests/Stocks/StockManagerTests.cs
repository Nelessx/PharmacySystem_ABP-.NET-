using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Categories;
using PharmacySystem.Medicines;
using PharmacySystem.Stocks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PharmacySystem.EntityFrameworkCore.Stocks;

[Collection(PharmacySystemTestConsts.CollectionDefinitionName)]
public class StockManagerTests : PharmacySystemEntityFrameworkCoreTestBase
{
    private readonly StockManager _stockManager;
    private readonly IRepository<Stock, Guid> _stockRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public StockManagerTests()
    {
        _stockManager = GetRequiredService<StockManager>();
        _stockRepository = GetRequiredService<IRepository<Stock, Guid>>();
        _medicineRepository = GetRequiredService<IRepository<Medicine, Guid>>();
        _categoryRepository = GetRequiredService<IRepository<Category, Guid>>();
    }

    // Persist a category + medicine so stock rows satisfy the Stock->Medicine FK.
    private async Task SeedMedicineAsync(Guid medicineId)
    {
        var categoryId = Guid.NewGuid();
        await _categoryRepository.InsertAsync(
            new Category(categoryId, "C" + categoryId.ToString("N")[..6], null, true), autoSave: true);
        await _medicineRepository.InsertAsync(
            new Medicine(medicineId, "M" + medicineId.ToString("N")[..6], categoryId, 1m, 2m), autoSave: true);
    }

    [Fact]
    public async Task IncreaseAsync_creates_a_new_lot()
    {
        var medicineId = Guid.NewGuid();
        var expiry = DateTime.Today.AddYears(1);

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", expiry, 10, 5m);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var stock = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            stock.ShouldNotBeNull();
            stock!.Quantity.ShouldBe(10);
            stock.UnitCost.ShouldBe(5m);
        });
    }

    [Fact]
    public async Task IncreaseAsync_same_batch_different_expiry_creates_distinct_lots()
    {
        var medicineId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", DateTime.Today.AddYears(1), 10, 5m);
            await _stockManager.IncreaseAsync(medicineId, "B1", DateTime.Today.AddYears(2), 20, 6m);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var lots = await _stockRepository.GetListAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            lots.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task IncreaseAsync_same_lot_accumulates_quantity()
    {
        var medicineId = Guid.NewGuid();
        var expiry = DateTime.Today.AddYears(1);

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", expiry, 10, 5m);
            await _stockManager.IncreaseAsync(medicineId, "B1", expiry, 15, 7m);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var lots = await _stockRepository.GetListAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            lots.Count.ShouldBe(1);
            lots.Single().Quantity.ShouldBe(25);
        });
    }

    [Fact]
    public async Task IncreaseQuantityAsync_preserves_unit_cost()
    {
        var medicineId = Guid.NewGuid();
        var expiry = DateTime.Today.AddYears(1);

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", expiry, 10, 5m);
            // Restoring quantity (e.g. reversing a sale) must not touch UnitCost.
            await _stockManager.IncreaseQuantityAsync(medicineId, "B1", expiry, 5);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var stock = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.BatchNumber == "B1");
            stock!.Quantity.ShouldBe(15);
            stock.UnitCost.ShouldBe(5m);
        });
    }

    [Fact]
    public async Task DecreaseAsync_deducts_from_the_matching_expiry_lot_only()
    {
        var medicineId = Guid.NewGuid();
        var e1 = DateTime.Today.AddYears(1);
        var e2 = DateTime.Today.AddYears(2);

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", e1, 10, 5m);
            await _stockManager.IncreaseAsync(medicineId, "B1", e2, 20, 6m);
            await _stockManager.DecreaseAsync(medicineId, "B1", e1, 4);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var lot1 = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.ExpiryDate == e1);
            var lot2 = await _stockRepository.FirstOrDefaultAsync(x => x.MedicineId == medicineId && x.ExpiryDate == e2);
            lot1!.Quantity.ShouldBe(6);
            lot2!.Quantity.ShouldBe(20);
        });
    }

    [Fact]
    public async Task DecreaseAsync_insufficient_stock_throws_business_exception()
    {
        var medicineId = Guid.NewGuid();
        var expiry = DateTime.Today.AddYears(1);

        await WithUnitOfWorkAsync(async () =>
        {
            await SeedMedicineAsync(medicineId);
            await _stockManager.IncreaseAsync(medicineId, "B1", expiry, 5, 5m);
        });

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => _stockManager.DecreaseAsync(medicineId, "B1", expiry, 10)));
        ex.Code.ShouldBe(PharmacySystemDomainErrorCodes.InsufficientStock);
    }

    [Fact]
    public async Task DecreaseAsync_missing_lot_throws_business_exception()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            WithUnitOfWorkAsync(() => _stockManager.DecreaseAsync(Guid.NewGuid(), "NOPE", null, 1)));
        ex.Code.ShouldBe(PharmacySystemDomainErrorCodes.StockNotFound);
    }
}
