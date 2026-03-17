using System;
using System.Linq;
using System.Threading.Tasks;
using PharmacySystem.Categories;
using PharmacySystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Medicines;

// Application service for Medicine module
public class MedicineAppService :
    CrudAppService<
        Medicine,
        MedicineDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateMedicineDto>,
    IMedicineAppService
{
    private readonly IRepository<Category, Guid> _categoryRepository;

    public MedicineAppService(
        IRepository<Medicine, Guid> repository,
        IRepository<Category, Guid> categoryRepository)
        : base(repository)
    {
        _categoryRepository = categoryRepository;

        GetPolicyName = PharmacySystemPermissions.Medicines.Default;
        GetListPolicyName = PharmacySystemPermissions.Medicines.Default;
        CreatePolicyName = PharmacySystemPermissions.Medicines.Create;
        UpdatePolicyName = PharmacySystemPermissions.Medicines.Edit;
        DeletePolicyName = PharmacySystemPermissions.Medicines.Delete;
    }

    protected override Task<Medicine> MapToEntityAsync(CreateUpdateMedicineDto input)
    {
        var medicine = new Medicine(
            GuidGenerator.Create(),
            input.Name,
            input.CategoryId,
            input.PurchasePrice,
            input.SalePrice,
            input.GenericName,
            input.Unit,
            input.Barcode,
            input.ReorderLevel,
            input.IsActive
        );

        return Task.FromResult(medicine);
    }

    protected override Task MapToEntityAsync(CreateUpdateMedicineDto input, Medicine entity)
    {
        entity.SetName(input.Name);
        entity.SetGenericName(input.GenericName);
        entity.SetCategory(input.CategoryId);
        entity.SetUnit(input.Unit);
        entity.SetBarcode(input.Barcode);
        entity.SetPrices(input.PurchasePrice, input.SalePrice);
        entity.SetReorderLevel(input.ReorderLevel);
        entity.SetIsActive(input.IsActive);

        return Task.CompletedTask;
    }

    public async Task<ListResultDto<CategoryLookupDto>> GetCategoryLookupAsync()
    {
        var categories = await _categoryRepository.GetListAsync();

        var items = categories
            .OrderBy(x => x.Name)
            .Select(x => new CategoryLookupDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        return new ListResultDto<CategoryLookupDto>(items);
    }

    public override async Task<MedicineDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var medicine = await Repository.GetAsync(id);
        var dto = ObjectMapper.Map<Medicine, MedicineDto>(medicine);

        var category = await _categoryRepository.FindAsync(medicine.CategoryId);
        dto.CategoryName = category?.Name;

        return dto;
    }

    public override async Task<PagedResultDto<MedicineDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var medicineQueryable = await Repository.GetQueryableAsync();
        var categoryQueryable = await _categoryRepository.GetQueryableAsync();

        var query =
            from medicine in medicineQueryable
            join category in categoryQueryable
                on medicine.CategoryId equals category.Id into medicineCategories
            from category in medicineCategories.DefaultIfEmpty()
            orderby medicine.Name
            select new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                GenericName = medicine.GenericName,
                CategoryId = medicine.CategoryId,
                CategoryName = category != null ? category.Name : null,
                Unit = medicine.Unit,
                Barcode = medicine.Barcode,
                PurchasePrice = medicine.PurchasePrice,
                SalePrice = medicine.SalePrice,
                ReorderLevel = medicine.ReorderLevel,
                IsActive = medicine.IsActive,
                CreationTime = medicine.CreationTime,
                CreatorId = medicine.CreatorId,
                LastModificationTime = medicine.LastModificationTime,
                LastModifierId = medicine.LastModifierId,
                IsDeleted = medicine.IsDeleted,
                DeleterId = medicine.DeleterId,
                DeletionTime = medicine.DeletionTime
            };

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<MedicineDto>(totalCount, items);
    }
}