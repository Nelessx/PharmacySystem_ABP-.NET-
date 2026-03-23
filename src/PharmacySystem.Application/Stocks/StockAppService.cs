using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Medicines;
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
    // Medicine repository is needed to fill MedicineName
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    public StockAppService(
        IReadOnlyRepository<Stock, Guid> repository,
        IRepository<Medicine, Guid> medicineRepository)
        : base(repository)
    {
        _medicineRepository = medicineRepository;
    }

    // Returns one stock record with medicine name
    public override async Task<StockDto> GetAsync(Guid id)
    {
        // Load stock entity
        var stock = await Repository.GetAsync(id);

        // Map basic fields
        var dto = ObjectMapper.Map<Stock, StockDto>(stock);

        // Fill readable medicine name manually
        var medicine = await _medicineRepository.FindAsync(stock.MedicineId);
        dto.MedicineName = medicine?.Name;

        return dto;
    }

    // Returns stock list with medicine names
    public override async Task<PagedResultDto<StockDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        // Load stock query
        var queryable = await Repository.GetQueryableAsync();

        // Default sorting: latest first
        var query = queryable.OrderByDescending(x => x.CreationTime);

        // Count before paging
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply paging
        var stocks = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Load medicines once
        var medicines = await _medicineRepository.GetListAsync();

        // Map and fill medicine name
        var items = stocks.Select(stock =>
        {
            var dto = ObjectMapper.Map<Stock, StockDto>(stock);

            var medicine = medicines.FirstOrDefault(x => x.Id == stock.MedicineId);
            dto.MedicineName = medicine?.Name;

            return dto;
        }).ToList();

        return new PagedResultDto<StockDto>(totalCount, items);
    }
}