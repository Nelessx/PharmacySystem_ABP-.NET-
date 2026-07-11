using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Customers;
using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using PharmacySystem.Stocks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
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
                item.BatchNumber,
                item.ExpiryDate
            );
        }

        sale.EnsureValid();

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
                item.BatchNumber,
                item.ExpiryDate
            );
        }

        entity.EnsureValid();

        await Task.CompletedTask;
    }

    // Create sale and then decrease stock for each sold item
    public override async Task<SaleDto> CreateAsync(CreateUpdateSaleDto input)
    {
        // Enforce the create permission before mutating any stock, so an
        // unauthorized caller can never trigger stock side effects.
        await CheckCreatePolicyAsync();

        // Friendly duplicate check before hitting the unique index.
        if (await Repository.AnyAsync(x => x.SaleNumber == input.SaleNumber))
        {
            throw new UserFriendlyException($"A sale with number '{input.SaleNumber}' already exists.");
        }

        try
        {
            // First decrease stock. If stock is insufficient, creation fails and
            // the ambient unit of work rolls the deductions back.
            foreach (var item in input.Items)
            {
                await _stockManager.DecreaseAsync(
                    item.MedicineId,
                    item.BatchNumber ?? throw new ArgumentException("Batch number is required for stock deduction."),
                    item.ExpiryDate,
                    item.Quantity
                );
            }

            // If stock deduction succeeded for all items, create and save the Sale
            return await base.CreateAsync(input);
        }
        catch (AbpDbConcurrencyException)
        {
            throw new UserFriendlyException(
                "The stock for one or more items changed while completing this sale. Please reload and try again.");
        }
    }

    // Returns active customers for the Sale dropdown (filtered/projected in SQL).
    public async Task<ListResultDto<CustomerLookupDto>> GetCustomerLookupAsync()
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Sales.Default);

        var queryable = await _customerRepository.GetQueryableAsync();

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new CustomerLookupDto { Id = x.Id, Name = x.Name })
        );

        return new ListResultDto<CustomerLookupDto>(items);
    }

    // Returns active medicines for the Sale item dropdown (filtered/projected in SQL).
    public async Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync()
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Sales.Default);

        var queryable = await _medicineRepository.GetQueryableAsync();

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new MedicineLookupDto { Id = x.Id, Name = x.Name })
        );

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

        // Fill medicine names in one query instead of one round-trip per item.
        var medicineNames = await GetMedicineNamesAsync(dto.Items.Select(x => x.MedicineId));
        foreach (var item in dto.Items)
        {
            item.MedicineName = medicineNames.GetValueOrDefault(item.MedicineId);
        }

        return dto;
    }

    // Loads a medicineId -> name map for the given ids in a single query.
    private async Task<Dictionary<Guid, string>> GetMedicineNamesAsync(IEnumerable<Guid> medicineIds)
    {
        var ids = medicineIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var queryable = await _medicineRepository.GetQueryableAsync();
        var medicines = await AsyncExecuter.ToListAsync(
            queryable.Where(m => ids.Contains(m.Id)).Select(m => new { m.Id, m.Name }));

        return medicines.ToDictionary(m => m.Id, m => m.Name);
    }

    // Returns sale list with customer name filled manually
    public override async Task<PagedResultDto<SaleDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Honor client-supplied sorting; default to newest-first.
        var query = string.IsNullOrWhiteSpace(input.Sorting)
            ? queryable.OrderByDescending(x => x.CreationTime)
            : ApplySorting(queryable, input);

        var sales = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Resolve names only for the customers referenced on this page.
        var customerIds = sales.Where(x => x.CustomerId.HasValue)
            .Select(x => x.CustomerId!.Value).Distinct().ToList();

        var customerNames = new Dictionary<Guid, string>();
        if (customerIds.Count > 0)
        {
            var customerQueryable = await _customerRepository.GetQueryableAsync();
            var customers = await AsyncExecuter.ToListAsync(
                customerQueryable.Where(c => customerIds.Contains(c.Id)).Select(c => new { c.Id, c.Name }));
            customerNames = customers.ToDictionary(c => c.Id, c => c.Name);
        }

        var items = sales.Select(sale =>
        {
            var dto = ObjectMapper.Map<Sale, SaleDto>(sale);

            if (sale.CustomerId.HasValue)
            {
                dto.CustomerName = customerNames.GetValueOrDefault(sale.CustomerId.Value);
            }

            return dto;
        }).ToList();

        return new PagedResultDto<SaleDto>(totalCount, items);
    }

    public override async Task<SaleDto> UpdateAsync(Guid id, CreateUpdateSaleDto input)
    {
        await CheckUpdatePolicyAsync();

        var sale = await Repository.GetAsync(id);

        // Apply the caller's concurrency stamp so a stale edit (or a concurrent
        // edit/delete) is rejected instead of silently double-applying stock.
        sale.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

        try
        {
            // 🔴 STEP 1: RESTORE OLD STOCK (quantity only — never overwrite the
            // batch's real purchase cost with the item's selling price)
            foreach (var item in sale.Items)
            {
                await _stockManager.IncreaseQuantityAsync(
                    item.MedicineId,
                    item.BatchNumber!,
                    item.ExpiryDate,
                    item.Quantity
                );
            }

            // 🔵 STEP 2: UPDATE ENTITY
            await MapToEntityAsync(input, sale);
            await Repository.UpdateAsync(sale, autoSave: true);

            // 🟢 STEP 3: APPLY NEW SALE (DEDUCT STOCK with ExpiryDate matching)
            foreach (var item in input.Items)
            {
                await _stockManager.DecreaseAsync(
                    item.MedicineId,
                    item.BatchNumber!,
                    item.ExpiryDate,
                    item.Quantity
                );
            }
        }
        catch (AbpDbConcurrencyException)
        {
            throw new UserFriendlyException(
                "This sale or its stock changed since you loaded it. Please reload and try again.");
        }

        return ObjectMapper.Map<Sale, SaleDto>(sale);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var sale = await Repository.GetAsync(id);

        // 🟢 Restore stock (quantity only — preserve the batch's purchase cost)
        foreach (var item in sale.Items)
        {
            await _stockManager.IncreaseQuantityAsync(
                item.MedicineId,
                item.BatchNumber!,
                item.ExpiryDate,
                item.Quantity
            );
        }

        await Repository.DeleteAsync(id);
    }
}