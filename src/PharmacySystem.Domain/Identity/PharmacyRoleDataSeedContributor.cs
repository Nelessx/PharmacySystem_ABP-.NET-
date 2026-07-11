using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace PharmacySystem.Identity;

/// <summary>
/// Seeds the standard pharmacy roles (Manager, Pharmacist, Cashier) with a
/// sensible scoped permission set, so a fresh installation has ready-to-use
/// roles beyond the all-powerful admin. Idempotent: safe to run repeatedly.
/// </summary>
/// <remarks>
/// This lives in the Domain layer (where Identity/PermissionManagement services
/// are available and the DbMigrator loads it). The permission name constants
/// below mirror <c>PharmacySystem.Permissions.PharmacySystemPermissions</c> in
/// the Application.Contracts layer, which the Domain project cannot reference.
/// </remarks>
public class PharmacyRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string GroupName = "PharmacySystem";

    private static class Perm
    {
        public const string Categories = GroupName + ".Categories";
        public const string Medicines = GroupName + ".Medicines";
        public const string Suppliers = GroupName + ".Suppliers";
        public const string Customers = GroupName + ".Customers";
        public const string Purchases = GroupName + ".Purchases";
        public const string Sales = GroupName + ".Sales";
        public const string Stock = GroupName + ".Stock";
        public const string Reports = GroupName + ".Reports";

        public const string Create = ".Create";
        public const string Edit = ".Edit";
        public const string Delete = ".Delete";
    }

    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PharmacyRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
    }

    // A shop manager can do everything within the pharmacy modules.
    private static readonly string[] ManagerPermissions =
    {
        Perm.Categories, Perm.Categories + Perm.Create, Perm.Categories + Perm.Edit, Perm.Categories + Perm.Delete,
        Perm.Medicines, Perm.Medicines + Perm.Create, Perm.Medicines + Perm.Edit, Perm.Medicines + Perm.Delete,
        Perm.Suppliers, Perm.Suppliers + Perm.Create, Perm.Suppliers + Perm.Edit, Perm.Suppliers + Perm.Delete,
        Perm.Customers, Perm.Customers + Perm.Create, Perm.Customers + Perm.Edit, Perm.Customers + Perm.Delete,
        Perm.Purchases, Perm.Purchases + Perm.Create, Perm.Purchases + Perm.Edit, Perm.Purchases + Perm.Delete,
        Perm.Sales, Perm.Sales + Perm.Create, Perm.Sales + Perm.Edit, Perm.Sales + Perm.Delete,
        Perm.Stock,
        Perm.Reports,
    };

    // A pharmacist dispenses medicines and manages day-to-day catalogue/stock,
    // but does not delete master data or run purchasing.
    private static readonly string[] PharmacistPermissions =
    {
        Perm.Categories,
        Perm.Medicines,
        Perm.Suppliers,
        Perm.Customers, Perm.Customers + Perm.Create, Perm.Customers + Perm.Edit,
        Perm.Purchases,
        Perm.Sales, Perm.Sales + Perm.Create, Perm.Sales + Perm.Edit,
        Perm.Stock,
        Perm.Reports,
    };

    // A cashier operates the POS: create sales and register customers, view catalogue/stock.
    private static readonly string[] CashierPermissions =
    {
        Perm.Medicines,
        Perm.Customers, Perm.Customers + Perm.Create,
        Perm.Sales, Perm.Sales + Perm.Create,
        Perm.Stock,
    };

    public async Task SeedAsync(DataSeedContext context)
    {
        // Batch every role + permission-grant write into a single unit of work
        // so there is one SaveChanges rather than dozens. Besides being more
        // efficient, this avoids the many small concurrent writes that can
        // contend for a shared (e.g. in-memory SQLite) connection during seeding.
        using (_currentTenant.Change(context?.TenantId))
        using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
        {
            await CreateRoleWithPermissionsAsync("Manager", ManagerPermissions);
            await CreateRoleWithPermissionsAsync("Pharmacist", PharmacistPermissions);
            await CreateRoleWithPermissionsAsync("Cashier", CashierPermissions);

            await uow.CompleteAsync();
        }
    }

    private async Task CreateRoleWithPermissionsAsync(string roleName, string[] permissions)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());

        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName, _currentTenant.Id)
            {
                IsStatic = true,
                IsPublic = true
            };

            (await _roleManager.CreateAsync(role)).CheckErrors();
        }

        foreach (var permission in permissions)
        {
            await _permissionManager.SetForRoleAsync(roleName, permission, true);
        }
    }
}
