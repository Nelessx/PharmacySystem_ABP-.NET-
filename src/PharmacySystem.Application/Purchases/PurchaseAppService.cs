using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using PharmacySystem.Stocks;
using PharmacySystem.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace PharmacySystem.Purchases;

// Application service for Purchase module
public class PurchaseAppService :
    CrudAppService<
        Purchase,
        PurchaseDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdatePurchaseDto>,
    IPurchaseAppService
{
    // Repository for Supplier lookup and supplier name loading
    private readonly IRepository<Supplier, Guid> _supplierRepository;

    // Repository for Medicine lookup and medicine name loading
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    // Domain service responsible for stock updates
    private readonly StockManager _stockManager;

    public PurchaseAppService(
        IRepository<Purchase, Guid> repository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Medicine, Guid> medicineRepository,
        StockManager stockManager)
        : base(repository)
    {
        _supplierRepository = supplierRepository;
        _medicineRepository = medicineRepository;
        _stockManager = stockManager;

        // Permission rules for Purchase module
        GetPolicyName = PharmacySystemPermissions.Purchases.Default;
        GetListPolicyName = PharmacySystemPermissions.Purchases.Default;
        CreatePolicyName = PharmacySystemPermissions.Purchases.Create;
        UpdatePolicyName = PharmacySystemPermissions.Purchases.Edit;
        DeletePolicyName = PharmacySystemPermissions.Purchases.Delete;
    }

    // Maps create/update DTO into a new Purchase entity
    protected override async Task<Purchase> MapToEntityAsync(CreateUpdatePurchaseDto input)
    {
        // Create Purchase header
        var purchase = new Purchase(
            GuidGenerator.Create(),
            input.PurchaseNumber,
            input.SupplierId,
            input.PurchaseDate,
            input.InvoiceNumber,
            input.Notes,
            input.DiscountAmount
        );

        // Add each item into the Purchase aggregate
        foreach (var item in input.Items)
        {
            purchase.AddItem(
                item.MedicineId,
                item.Quantity,
                item.UnitPrice,
                item.BatchNumber,
                item.ExpiryDate
            );
        }

        purchase.EnsureValid();

        return await Task.FromResult(purchase);
    }

    // Maps create/update DTO into an existing Purchase entity
    protected override async Task MapToEntityAsync(CreateUpdatePurchaseDto input, Purchase entity)
    {
        // Update Purchase header
        entity.SetPurchaseNumber(input.PurchaseNumber);
        entity.SetSupplier(input.SupplierId);
        entity.SetPurchaseDate(input.PurchaseDate);
        entity.SetInvoiceNumber(input.InvoiceNumber);
        entity.SetNotes(input.Notes);
        entity.SetDiscountAmount(input.DiscountAmount);

        // Update items collection
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

    // Create purchase and then increase stock for each purchase item
    public override async Task<PurchaseDto> CreateAsync(CreateUpdatePurchaseDto input)
    {
        // Friendly duplicate check before hitting the unique index.
        if (await Repository.AnyAsync(x => x.PurchaseNumber == input.PurchaseNumber))
        {
            throw new UserFriendlyException($"A purchase with number '{input.PurchaseNumber}' already exists.");
        }

        // Let ABP create and save the Purchase first
        var result = await base.CreateAsync(input);

        // After successful save, increase stock for each purchased item
        foreach (var item in input.Items)
        {
            await _stockManager.IncreaseAsync(
                item.MedicineId,
                item.BatchNumber ?? throw new ArgumentException("Batch number is required for stock."),
                item.ExpiryDate,
                item.Quantity,
                item.UnitPrice
            );
        }

        return result;
    }

    // Returns suppliers for the Purchase dropdown (ordered/projected in SQL).
    public async Task<ListResultDto<SupplierLookupDto>> GetSupplierLookupAsync()
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Purchases.Default);

        var queryable = await _supplierRepository.GetQueryableAsync();

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(x => x.Name)
                .Select(x => new SupplierLookupDto { Id = x.Id, Name = x.Name })
        );

        return new ListResultDto<SupplierLookupDto>(items);
    }

    // Returns active medicines for the Purchase item dropdown (projected in SQL).
    public async Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync()
    {
        await CheckPolicyAsync(PharmacySystemPermissions.Purchases.Default);

        var queryable = await _medicineRepository.GetQueryableAsync();

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new MedicineLookupDto { Id = x.Id, Name = x.Name })
        );

        return new ListResultDto<MedicineLookupDto>(items);
    }

    // Returns one purchase with supplier name and medicine names filled manually
    public override async Task<PurchaseDto> GetAsync(Guid id)
    {
        // Check permission first
        await CheckGetPolicyAsync();

        // Load purchase entity
        var purchase = await Repository.GetAsync(id);

        // Map basic Purchase fields using Mapperly
        var dto = ObjectMapper.Map<Purchase, PurchaseDto>(purchase);

        // Manually fill supplier name because Purchase entity only stores SupplierId
        var supplier = await _supplierRepository.FindAsync(purchase.SupplierId);
        dto.SupplierName = supplier?.Name;

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

    // Returns purchase list with supplier name filled manually
    public override async Task<PagedResultDto<PurchaseDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        // Check permission first
        await CheckGetListPolicyAsync();

        // Load purchases query
        var queryable = await Repository.GetQueryableAsync();

        // Get total count before paging
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var purchases = await GetSortedPurchasePageAsync(queryable, input);

        // Resolve names only for the suppliers referenced on this page.
        var supplierIds = purchases.Select(x => x.SupplierId).Distinct().ToList();

        var supplierNames = new Dictionary<Guid, string>();
        if (supplierIds.Count > 0)
        {
            var supplierQueryable = await _supplierRepository.GetQueryableAsync();
            var suppliers = await AsyncExecuter.ToListAsync(
                supplierQueryable.Where(s => supplierIds.Contains(s.Id)).Select(s => new { s.Id, s.Name }));
            supplierNames = suppliers.ToDictionary(s => s.Id, s => s.Name);
        }

        var items = purchases.Select(purchase =>
        {
            var dto = ObjectMapper.Map<Purchase, PurchaseDto>(purchase);
            dto.SupplierName = supplierNames.GetValueOrDefault(purchase.SupplierId);
            return dto;
        }).ToList();

        return new PagedResultDto<PurchaseDto>(totalCount, items);
    }

    // Applies paging + sorting to the purchase query and returns the requested page.
    //
    // "supplierName" is a display field resolved from the Supplier table, not a
    // column on the Purchase entity, so the base ApplySorting cannot handle it and
    // would throw. Translate it into a join-based sort in SQL; fall back to the
    // base behaviour for real entity columns and newest-first when unsorted.
    private async Task<List<Purchase>> GetSortedPurchasePageAsync(
        IQueryable<Purchase> queryable, PagedAndSortedResultRequestDto input)
    {
        var sorting = input.Sorting?.Trim();

        if (!string.IsNullOrWhiteSpace(sorting) &&
            sorting.StartsWith("supplierName", StringComparison.OrdinalIgnoreCase))
        {
            var descending = sorting.EndsWith("desc", StringComparison.OrdinalIgnoreCase);
            var supplierQueryable = await _supplierRepository.GetQueryableAsync();

            var joined = from purchase in queryable
                         join supplier in supplierQueryable on purchase.SupplierId equals supplier.Id
                         select new { Purchase = purchase, supplier.Name };

            joined = descending
                ? joined.OrderByDescending(x => x.Name).ThenBy(x => x.Purchase.Id)
                : joined.OrderBy(x => x.Name).ThenBy(x => x.Purchase.Id);

            return await AsyncExecuter.ToListAsync(
                joined
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .Select(x => x.Purchase));
        }

        var query = string.IsNullOrWhiteSpace(sorting)
            ? queryable.OrderByDescending(x => x.CreationTime)
            : ApplySorting(queryable, input);

        return await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));
    }

    public override async Task<PurchaseDto> UpdateAsync(Guid id, CreateUpdatePurchaseDto input)
    {
        // Load existing purchase WITH items
        var purchase = await Repository.GetAsync(id);

        // 🔴 STEP 1: REVERSE OLD STOCK (before any modifications)
        var oldItems = purchase.Items.ToList();
        foreach (var item in oldItems)
        {
            await _stockManager.DecreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                item.ExpiryDate,
                item.Quantity
            );
        }

        // 🔵 STEP 2: UPDATE ENTITY
        // Set the concurrency stamp from input - this is critical for optimistic concurrency
        purchase.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);
        await MapToEntityAsync(input, purchase);

        // 🔐 Try to update with optimistic concurrency check
        // If the stamp doesn't match, EF Core will throw DbUpdateConcurrencyException
        try
        {
            await Repository.UpdateAsync(purchase, autoSave: true);
        }
        catch (AbpDbConcurrencyException)
        {
            // Concurrency conflict detected - re-throw so client can handle it
            throw;
        }

        // 🟢 STEP 3: APPLY NEW STOCK (only after successful update)
        foreach (var item in input.Items)
        {
            await _stockManager.IncreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                item.ExpiryDate,
                item.Quantity,
                item.UnitPrice
            );
        }

        return ObjectMapper.Map<Purchase, PurchaseDto>(purchase);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var purchase = await Repository.GetAsync(id);

        // 🔴 Reverse stock
        foreach (var item in purchase.Items)
        {
            await _stockManager.DecreaseAsync(
                item.MedicineId,
                item.BatchNumber!,
                item.ExpiryDate,
                item.Quantity
            );
        }

        await Repository.DeleteAsync(id);
    }
}