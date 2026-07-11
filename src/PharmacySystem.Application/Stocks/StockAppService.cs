using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Stocks;

// Read-only application service for Stock module
public class StockAppService :
    ReadOnlyAppService<
        Stock,
        StockDto,
        Guid,
        PagedAndSortedResultRequestDto>,
    IStockAppService
{
    // Medicine repository is needed to fill MedicineName and ReorderLevel
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    public StockAppService(
        IReadOnlyRepository<Stock, Guid> repository,
        IRepository<Medicine, Guid> medicineRepository)
        : base(repository)
    {
        _medicineRepository = medicineRepository;

        // Require the Stock permission for Get/GetList and the custom endpoints below.
        GetPolicyName = PharmacySystemPermissions.Stock.Default;
        GetListPolicyName = PharmacySystemPermissions.Stock.Default;
    }

    // Returns one stock record with medicine name
    public override async Task<StockDto> GetAsync(Guid id)
    {
        var stock = await Repository.GetAsync(id);

        var dto = ObjectMapper.Map<Stock, StockDto>(stock);

        var medicine = await _medicineRepository.FindAsync(stock.MedicineId);
        dto.MedicineName = medicine?.Name;

        return dto;
    }

    // Returns stock list with medicine names
    public override async Task<PagedResultDto<StockDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await Repository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var stocks = await GetSortedStockPageAsync(queryable, input);

        // Resolve names only for the medicines referenced on this page.
        var medicineIds = stocks.Select(x => x.MedicineId).Distinct().ToList();

        var medicineNames = new Dictionary<Guid, string>();
        if (medicineIds.Count > 0)
        {
            var medicineQueryable = await _medicineRepository.GetQueryableAsync();
            var medicines = await AsyncExecuter.ToListAsync(
                medicineQueryable.Where(m => medicineIds.Contains(m.Id)).Select(m => new { m.Id, m.Name }));
            medicineNames = medicines.ToDictionary(m => m.Id, m => m.Name);
        }

        var items = stocks.Select(stock =>
        {
            var dto = ObjectMapper.Map<Stock, StockDto>(stock);
            dto.MedicineName = medicineNames.GetValueOrDefault(stock.MedicineId);
            return dto;
        }).ToList();

        return new PagedResultDto<StockDto>(totalCount, items);
    }

    // Applies paging + sorting to the stock query and returns the requested page.
    //
    // "medicineName" is a display field resolved from the Medicine table, not a
    // column on the Stock entity, so the base ApplySorting (which sorts the Stock
    // query directly via dynamic LINQ) cannot handle it and would throw
    // "No property or field 'medicineName' exists in type 'Stock'". Translate it
    // into a join-based sort in SQL so paging stays correct; fall back to the
    // base behaviour for real entity columns and newest-first when unsorted.
    private async Task<List<Stock>> GetSortedStockPageAsync(
        IQueryable<Stock> queryable, PagedAndSortedResultRequestDto input)
    {
        var sorting = input.Sorting?.Trim();

        if (!string.IsNullOrWhiteSpace(sorting) &&
            sorting.StartsWith("medicineName", StringComparison.OrdinalIgnoreCase))
        {
            var descending = sorting.EndsWith("desc", StringComparison.OrdinalIgnoreCase);
            var medicineQueryable = await _medicineRepository.GetQueryableAsync();

            var joined = from stock in queryable
                         join medicine in medicineQueryable on stock.MedicineId equals medicine.Id
                         select new { Stock = stock, medicine.Name };

            joined = descending
                ? joined.OrderByDescending(x => x.Name).ThenBy(x => x.Stock.Id)
                : joined.OrderBy(x => x.Name).ThenBy(x => x.Stock.Id);

            return await AsyncExecuter.ToListAsync(
                joined
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .Select(x => x.Stock));
        }

        var query = string.IsNullOrWhiteSpace(sorting)
            ? queryable.OrderByDescending(x => x.CreationTime)
            : ApplySorting(queryable, input);

        return await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));
    }

    // Returns stock rows where quantity is below or equal to medicine reorder level
    public async Task<ListResultDto<LowStockDto>> GetLowStockAsync()
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Stock.Default);

        var stocks = await Repository.GetListAsync();
        var medicines = await _medicineRepository.GetListAsync();

        var items = stocks
            .Join(
                medicines,
                stock => stock.MedicineId,
                medicine => medicine.Id,
                (stock, medicine) => new LowStockDto
                {
                    StockId = stock.Id,
                    MedicineId = stock.MedicineId,
                    MedicineName = medicine.Name,
                    BatchNumber = stock.BatchNumber,
                    ExpiryDate = stock.ExpiryDate,
                    Quantity = stock.Quantity,
                    ReorderLevel = medicine.ReorderLevel
                }
            )
            .Where(x => x.Quantity <= x.ReorderLevel)
            .OrderBy(x => x.Quantity)
            .ToList();

        return new ListResultDto<LowStockDto>(items);
    }

    // Returns stock rows that are expired or expiring within the given number of days
    public async Task<ListResultDto<ExpiringStockDto>> GetExpiringStockAsync(int days = 30)
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Stock.Default);

        var stocks = await Repository.GetListAsync();
        var medicines = await _medicineRepository.GetListAsync();

        var today = DateTime.Today;

        var items = stocks
            .Where(x => x.ExpiryDate.HasValue)
            .Join(
                medicines,
                stock => stock.MedicineId,
                medicine => medicine.Id,
                (stock, medicine) => new ExpiringStockDto
                {
                    StockId = stock.Id,
                    MedicineId = stock.MedicineId,
                    MedicineName = medicine.Name,
                    BatchNumber = stock.BatchNumber,
                    ExpiryDate = stock.ExpiryDate,
                    Quantity = stock.Quantity,
                    DaysToExpire = stock.ExpiryDate.HasValue
                        ? (stock.ExpiryDate.Value.Date - today).Days
                        : int.MaxValue,
                    IsExpired = stock.ExpiryDate.HasValue && stock.ExpiryDate.Value.Date < today
                }
            )
            .Where(x => x.IsExpired || x.DaysToExpire <= days)
            .OrderBy(x => x.IsExpired ? 0 : 1)
            .ThenBy(x => x.DaysToExpire)
            .ToList();

        return new ListResultDto<ExpiringStockDto>(items);
    }
}