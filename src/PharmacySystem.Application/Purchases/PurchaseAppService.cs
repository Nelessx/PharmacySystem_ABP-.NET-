using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using PharmacySystem.Stocks;
using PharmacySystem.Suppliers;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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

        await Task.CompletedTask;
    }

    // Create purchase and then increase stock for each purchase item
    public override async Task<PurchaseDto> CreateAsync(CreateUpdatePurchaseDto input)
    {
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

    // Returns suppliers for Purchase dropdown
    public async Task<ListResultDto<SupplierLookupDto>> GetSupplierLookupAsync()
    {
        // Get all suppliers from database
        var suppliers = await _supplierRepository.GetListAsync();

        // Convert to lightweight lookup DTO
        var items = suppliers
            .OrderBy(x => x.Name)
            .Select(x => new SupplierLookupDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return new ListResultDto<SupplierLookupDto>(items);
    }

    // Returns medicines for Purchase item dropdown
    public async Task<ListResultDto<MedicineLookupDto>> GetMedicineLookupAsync()
    {
        // Get all medicines from database
        var medicines = await _medicineRepository.GetListAsync();

        // Convert to lightweight lookup DTO
        var items = medicines
            .OrderBy(x => x.Name)
            .Select(x => new MedicineLookupDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

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

        // Manually fill medicine names for each item
        foreach (var item in dto.Items)
        {
            var medicine = await _medicineRepository.FindAsync(item.MedicineId);
            item.MedicineName = medicine?.Name;
        }

        return dto;
    }

    // Returns purchase list with supplier name filled manually
    public override async Task<PagedResultDto<PurchaseDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        // Check permission first
        await CheckGetListPolicyAsync();

        // Load purchases query
        var queryable = await Repository.GetQueryableAsync();

        // Simple default sorting for now
        var query = queryable.OrderByDescending(x => x.CreationTime);

        // Get total count before paging
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply paging
        var purchases = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // Load suppliers once to avoid repeated DB calls inside loop
        var suppliers = await _supplierRepository.GetListAsync();

        // Map purchases and manually fill supplier name
        var items = purchases.Select(purchase =>
        {
            var dto = ObjectMapper.Map<Purchase, PurchaseDto>(purchase);

            var supplier = suppliers.FirstOrDefault(x => x.Id == purchase.SupplierId);
            dto.SupplierName = supplier?.Name;

            return dto;
        }).ToList();

        return new PagedResultDto<PurchaseDto>(totalCount, items);
    }
}