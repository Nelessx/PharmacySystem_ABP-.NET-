using System;
using System.Threading.Tasks;
using PharmacySystem.Permissions;
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
    public CategoryAppService(IRepository<Category, Guid> repository)
        : base(repository)
    {
        GetPolicyName = PharmacySystemPermissions.Categories.Default;
        GetListPolicyName = PharmacySystemPermissions.Categories.Default;
        CreatePolicyName = PharmacySystemPermissions.Categories.Create;
        UpdatePolicyName = PharmacySystemPermissions.Categories.Edit;
        DeletePolicyName = PharmacySystemPermissions.Categories.Delete;
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