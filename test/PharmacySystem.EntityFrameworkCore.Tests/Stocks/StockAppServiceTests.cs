using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Categories;
using PharmacySystem.Medicines;
using PharmacySystem.Stocks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PharmacySystem.EntityFrameworkCore.Stocks;

[Collection(PharmacySystemTestConsts.CollectionDefinitionName)]
public class StockAppServiceTests : PharmacySystemEntityFrameworkCoreTestBase
{
    private readonly IStockAppService _stockAppService;
    private readonly StockManager _stockManager;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;

    public StockAppServiceTests()
    {
        _stockAppService = GetRequiredService<IStockAppService>();
        _stockManager = GetRequiredService<StockManager>();
        _medicineRepository = GetRequiredService<IRepository<Medicine, Guid>>();
        _categoryRepository = GetRequiredService<IRepository<Category, Guid>>();
    }

    // Persist a category + named medicine with one stock lot.
    private async Task SeedMedicineWithStockAsync(string medicineName, string batch)
    {
        var categoryId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            await _categoryRepository.InsertAsync(
                new Category(categoryId, "Cat-" + categoryId.ToString("N")[..6], null, true), autoSave: true);
            await _medicineRepository.InsertAsync(
                new Medicine(medicineId, medicineName, categoryId, 1m, 2m), autoSave: true);
            await _stockManager.IncreaseAsync(medicineId, batch, DateTime.Today.AddYears(1), 10, 1m);
        });
    }

    // Regression: the POS requests the stock list sorted by "medicineName", which
    // is a DTO-only field (resolved from the Medicine table), not a Stock column.
    // Before the fix this threw "No property or field 'medicineName' exists in
    // type 'Stock'" and the endpoint returned 500 ("Failed to load stock").
    [Fact]
    public async Task GetListAsync_can_sort_by_medicineName_ascending()
    {
        await SeedMedicineWithStockAsync("Zebra Tablet", "BZ");
        await SeedMedicineWithStockAsync("Alpha Tablet", "BA");

        var result = await _stockAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { Sorting = "medicineName", MaxResultCount = 1000 });

        result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldAllBe(x => x.MedicineName != null);

        var names = result.Items.Select(x => x.MedicineName).ToList();
        names.ShouldBe(names.OrderBy(n => n, StringComparer.Ordinal).ToList());
    }

    [Fact]
    public async Task GetListAsync_can_sort_by_medicineName_descending()
    {
        await SeedMedicineWithStockAsync("Zeta Tablet", "BZ2");
        await SeedMedicineWithStockAsync("Beta Tablet", "BB2");

        var result = await _stockAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { Sorting = "medicineName desc", MaxResultCount = 1000 });

        var names = result.Items.Select(x => x.MedicineName).ToList();
        names.ShouldBe(names.OrderByDescending(n => n, StringComparer.Ordinal).ToList());
    }

    // A real Stock column must still sort via the base behaviour.
    [Fact]
    public async Task GetListAsync_can_sort_by_entity_column()
    {
        await SeedMedicineWithStockAsync("Gamma Tablet", "BG");

        var result = await _stockAppService.GetListAsync(
            new PagedAndSortedResultRequestDto { Sorting = "quantity desc", MaxResultCount = 1000 });

        result.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
    }
}
