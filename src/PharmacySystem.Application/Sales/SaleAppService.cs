using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using PharmacySystem.Stocks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Sales;

// Application service for Sale module
public class SaleAppService :
    CrudAppService<
        Sale,
        SaleDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateSaleDto>,
    ISaleAppService
{
    // Repository for Customer lookup and display name loading
    private readonly IRepository<Customer, Guid> _customerRepository;

    // Repository for Medicine lookup and display name loading
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    // Domain service responsible for stock updates
    private readonly StockManager _stockManager;

    public SaleAppService(
        IRepository<Sale, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Medicine, Guid> medicineRepository,
        StockManager stockManager)
        : base(repository)
    {
        _customerRepository = customerRepository;
        _medicineRepository = medicineRepository;
        _stockManager = stockManager;

        // Permission rules for Sales module
        GetPolicyName = PharmacySystemPermissions.Sales.Default;
        GetListPolicyName = PharmacySystemPermissions.Sales.Default;
        CreatePolicyName = PharmacySystemPermissions.Sales.Create;
        UpdatePolicyName = PharmacySystemPermissions.Sales.Edit;
        DeletePolicyName = PharmacySystemPermissions.Sales.Delete;
    }

    // Maps create/update DTO into a new Sale entity
    protected override async Task<Sale> MapToEntityAsync(CreateUpdateSaleDto input)
    {
        // Create Sale header
        var sale = new Sale(
            GuidGenerator.Create(),
            input.SaleNumber,
            input.SaleDate,
            input.CustomerId,
            input.Notes,
            input.DiscountAmount
        );

        // Add each item into the Sale aggregate
        foreach (var item in input.Items)
        {
            sale.AddItem(
                item.MedicineId,
                item.Quantity,
                item.UnitPrice,
                item.BatchNumber
            );
        }

        return await Task.FromResult(sale);
    }

    // Maps create/update DTO into an existing Sale entity
    protected override async Task MapToEntityAsync(CreateUpdateSaleDto input, Sale entity)
    {
        // Update Sale header
        entity.SetSaleNumber(input.SaleNumber);
        entity.SetSaleDate(input.SaleDate);
        entity.SetCustomer(input.CustomerId);
        entity.SetNotes(input.Notes);
        entity.SetDiscountAmount(input.DiscountAmount);

        // Clear old items and rebuild from input
        entity.ClearItems();

        foreach (var item in input.Items)
        {
            entity.AddItem(
                item.MedicineId,
                item.Quantity,
                item.UnitPrice,
                item.BatchNumber
            );
        }

        await Task.CompletedTask;
    }

    // Create sale and then decrease stock for each sold item
    public override async Task<SaleDto> CreateAsync(CreateUpdateSaleDto input)
    {
        // First decrease stock. If stock is insufficient, creation should fail.
        foreach (var item in input.Items)
        {
            await _stockManager.DecreaseAsync(
                item.MedicineId,
                item.BatchNumber ?? throw new ArgumentException("Batch number is required for stock deduction."),
                null,
                item.Quantity
            );
        }

        // If stock deduction succeeded for all items, create and save the Sale
        var result = await base.CreateAsync(input);

        return result;
    }

    // Returns customers for Sale dropdown
    public async Task<ListResultDto<CustomerLookupDto>> GetCustomerLookupAsync()
    {
        var customers = await _customerRepository.GetListAsync();

        var items = customers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CustomerLookupDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return new ListResultDto<CustomerLookupDto>(items);
    }

    // Returns medicines for Sale item dropdown
    public async Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync()
    {
        var medicines = await _medicineRepository.GetListAsync();

        var items = medicines
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new MedicineLookupDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return new ListResultDto<MedicineLookupDto>(items);
    }

    // Returns one sale with customer name and medicine names filled manually
    public override async Task<SaleDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var sale = await Repository.GetAsync(id);

        var dto = ObjectMapper.Map<Sale, SaleDto>(sale);

        // Fill customer name
        if (sale.CustomerId.HasValue)
        {
            var customer = await _customerRepository.FindAsync(sale.CustomerId.Value);
            dto.CustomerName = customer?.Name;
        }

        // Fill medicine names
        foreach (var item in dto.Items)
        {
            var medicine = await _medicineRepository.FindAsync(item.MedicineId);
            item.MedicineName = medicine?.Name;
        }

        return dto;
    }

    // Returns sale list with customer name filled manually
    public override async Task<PagedResultDto<SaleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var query = queryable.OrderByDescending(x => x.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sales = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );
            
        var customers = await _customerRepository.GetListAsync();

        var items = sales.Select(sale =>
        {
            var dto = ObjectMapper.Map<Sale, SaleDto>(sale);

            if (sale.CustomerId.HasValue)
            {
                var customer = customers.FirstOrDefault(x => x.Id == sale.CustomerId.Value);
                dto.CustomerName = customer?.Name;
            }

            return dto;
        }).ToList();

        return new PagedResultDto<SaleDto>(totalCount, items);
    }

    public override async Task<SaleDto> UpdateAsync(Guid id, CreateUpdateSaleDto input)
    {
        var sale = await Repository.GetAsync(id);

        // 🔴 STEP 1: RESTORE OLD STOCK
        foreach (var item in sale.Items)
        {
            await _stockManager.IncreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                null,
                item.Quantity,
                item.UnitPrice
            );
        }

        // 🔵 STEP 2: UPDATE ENTITY
        await MapToEntityAsync(input, sale);
        await Repository.UpdateAsync(sale, autoSave: true);

        // 🔴 STEP 3: APPLY NEW SALE (DEDUCT STOCK)
        foreach (var item in input.Items)
        {
            await _stockManager.DecreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                null,
                item.Quantity
            );
        }

        return ObjectMapper.Map<Sale, SaleDto>(sale);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var sale = await Repository.GetAsync(id);

        // 🔵 Restore stock
        foreach (var item in sale.Items)
        {
            await _stockManager.IncreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                null,
                item.Quantity,
                item.UnitPrice
            );
        }

        await Repository.DeleteAsync(id);
    }
}