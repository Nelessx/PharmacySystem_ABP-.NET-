using System;
using System.Threading.Tasks;
using PharmacySystem.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Customers;

// Application service for Customer module
public class CustomerAppService :
    CrudAppService<
        Customer,
        CustomerDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCustomerDto>,
    ICustomerAppService
{
    public CustomerAppService(IRepository<Customer, Guid> repository)
        : base(repository)
    {
        GetPolicyName = PharmacySystemPermissions.Customers.Default;
        GetListPolicyName = PharmacySystemPermissions.Customers.Default;
        CreatePolicyName = PharmacySystemPermissions.Customers.Create;
        UpdatePolicyName = PharmacySystemPermissions.Customers.Edit;
        DeletePolicyName = PharmacySystemPermissions.Customers.Delete;
    }

    protected override Task<Customer> MapToEntityAsync(CreateUpdateCustomerDto input)
    {
        var customer = new Customer(
            GuidGenerator.Create(),
            input.Name,
            input.Phone,
            input.Address,
            input.Gender,
            input.DateOfBirth,
            input.PatientCode,
            input.IsActive
        );

        return Task.FromResult(customer);
    }

    protected override Task MapToEntityAsync(CreateUpdateCustomerDto input, Customer entity)
    {
        entity.SetName(input.Name);
        entity.SetPhone(input.Phone);
        entity.SetAddress(input.Address);
        entity.SetGender(input.Gender);
        entity.SetDateOfBirth(input.DateOfBirth);
        entity.SetPatientCode(input.PatientCode);
        entity.SetIsActive(input.IsActive);

        return Task.CompletedTask;
    }
}