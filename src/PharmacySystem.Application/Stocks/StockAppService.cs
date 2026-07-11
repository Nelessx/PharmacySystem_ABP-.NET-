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

        // Honor client-supplied sorting; default to newest-first.
        var query = string.IsNullOrWhiteSpace(input.Sorting)
            ? queryable.OrderByDescending(x => x.CreationTime)
            : ApplySorting(queryable, input);

        var stocks = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

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