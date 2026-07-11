using System;
using System.Threading.Tasks;
using PharmacySystem.Medicines;
using PharmacySystem.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Categories;

public class CategoryAppService :
    CrudAppService<
        Category,
        CategoryDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCategoryDto>,
    ICategoryAppService
{
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    public CategoryAppService(
        IRepository<Category, Guid> repository,
        IRepository<Medicine, Guid> medicineRepository)
        : base(repository)
    {
        _medicineRepository = medicineRepository;

        GetPolicyName = PharmacySystemPermissions.Categories.Default;
        GetListPolicyName = PharmacySystemPermissions.Categories.Default;
        CreatePolicyName = PharmacySystemPermissions.Categories.Create;
        UpdatePolicyName = PharmacySystemPermissions.Categories.Edit;
        DeletePolicyName = PharmacySystemPermissions.Categories.Delete;
    }

    // Block deleting a category that medicines still reference, with a friendly
    // message instead of a foreign-key violation.
    public override async Task DeleteAsync(Guid id)
    {
        if (await _medicineRepository.AnyAsync(x => x.CategoryId == id))
        {
            throw new BusinessException(PharmacySystemDomainErrorCodes.CategoryInUse);
        }

        await base.DeleteAsync(id);
    }

    protected override Task<Category> MapToEntityAsync(CreateUpdateCategoryDto input)
    {
        var category = new Category(
            GuidGenerator.Create(),
            input.Name,
            input.Description,
            input.IsActive
        );

        return Task.FromResult(category);
    }

    protected override Task MapToEntityAsync(CreateUpdateCategoryDto input, Category entity)
    {
        entity.SetName(input.Name);
        entity.SetDescription(input.Description);
        entity.SetIsActive(input.IsActive);

        return Task.CompletedTask;
    }
}