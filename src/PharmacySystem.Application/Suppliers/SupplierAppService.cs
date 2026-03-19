using System;
using System.Threading.Tasks;
using PharmacySystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Suppliers;

// Application service for Supplier module
public class SupplierAppService :
    CrudAppService<
        Supplier,
        SupplierDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateSupplierDto>,
    ISupplierAppService
{
    public SupplierAppService(IRepository<Supplier, Guid> repository)
        : base(repository)
    {
        GetPolicyName = PharmacySystemPermissions.Suppliers.Default;
        GetListPolicyName = PharmacySystemPermissions.Suppliers.Default;
        CreatePolicyName = PharmacySystemPermissions.Suppliers.Create;
        UpdatePolicyName = PharmacySystemPermissions.Suppliers.Edit;
        DeletePolicyName = PharmacySystemPermissions.Suppliers.Delete;
    }

    protected override Task<Supplier> MapToEntityAsync(CreateUpdateSupplierDto input)
    {
        var supplier = new Supplier(
            GuidGenerator.Create(),
            input.Name,
            input.ContactPerson,
            input.Phone,
            input.Email,
            input.Address,
            input.IsActive
        );

        return Task.FromResult(supplier);
    }

    protected override Task MapToEntityAsync(CreateUpdateSupplierDto input, Supplier entity)
    {
        entity.SetName(input.Name);
        entity.SetContactPerson(input.ContactPerson);
        entity.SetPhone(input.Phone);
        entity.SetEmail(input.Email);
        entity.SetAddress(input.Address);
        entity.SetIsActive(input.IsActive);

        return Task.CompletedTask;
    }
}