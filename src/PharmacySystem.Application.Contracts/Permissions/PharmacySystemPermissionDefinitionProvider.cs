using PharmacySystem.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace PharmacySystem.Permissions;

public class PharmacySystemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var pharmacyGroup = context.AddGroup(
            PharmacySystemPermissions.GroupName,
            L("Permission:PharmacySystem")
        );

        var categories = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Categories.Default,
            L("Permission:Categories")
        );
        categories.AddChild(
            PharmacySystemPermissions.Categories.Create,
            L("Permission:Categories.Create")
        );
        categories.AddChild(
            PharmacySystemPermissions.Categories.Edit,
            L("Permission:Categories.Edit")
        );
        categories.AddChild(
            PharmacySystemPermissions.Categories.Delete,
            L("Permission:Categories.Delete")
        );

        var medicines = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Medicines.Default,
            L("Permission:Medicines")
        );
        medicines.AddChild(
            PharmacySystemPermissions.Medicines.Create,
            L("Permission:Medicines.Create")
        );
        medicines.AddChild(
            PharmacySystemPermissions.Medicines.Edit,
            L("Permission:Medicines.Edit")
        );
        medicines.AddChild(
            PharmacySystemPermissions.Medicines.Delete,
            L("Permission:Medicines.Delete")
        );

        var suppliers = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Suppliers.Default,
            L("Permission:Suppliers")
        );
        suppliers.AddChild(
            PharmacySystemPermissions.Suppliers.Create,
            L("Permission:Suppliers.Create")
        );
        suppliers.AddChild(
            PharmacySystemPermissions.Suppliers.Edit,
            L("Permission:Suppliers.Edit")
        );
        suppliers.AddChild(
            PharmacySystemPermissions.Suppliers.Delete,
            L("Permission:Suppliers.Delete")
        );

        var customers = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Customers.Default,
            L("Permission:Customers")
        );
        customers.AddChild(
            PharmacySystemPermissions.Customers.Create,
            L("Permission:Customers.Create")
        );
        customers.AddChild(
            PharmacySystemPermissions.Customers.Edit,
            L("Permission:Customers.Edit")
        );
        customers.AddChild(
            PharmacySystemPermissions.Customers.Delete,
            L("Permission:Customers.Delete")
        );

        var purchases = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Purchases.Default,
            L("Permission:Purchases")
        );
        purchases.AddChild(
            PharmacySystemPermissions.Purchases.Create,
            L("Permission:Purchases.Create")
        );
        purchases.AddChild(
            PharmacySystemPermissions.Purchases.Edit,
            L("Permission:Purchases.Edit")
        );
        purchases.AddChild(
            PharmacySystemPermissions.Purchases.Delete,
            L("Permission:Purchases.Delete")
        );

        var sales = pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Sales.Default,
            L("Permission:Sales")
        );
        sales.AddChild(
            PharmacySystemPermissions.Sales.Create,
            L("Permission:Sales.Create")
        );
        sales.AddChild(
            PharmacySystemPermissions.Sales.Edit,
            L("Permission:Sales.Edit")
        );
        sales.AddChild(
            PharmacySystemPermissions.Sales.Delete,
            L("Permission:Sales.Delete")
        );

        pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Stock.Default,
            L("Permission:Stock")
        );

        pharmacyGroup.AddPermission(
            PharmacySystemPermissions.Reports.Default,
            L("Permission:Reports")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PharmacySystemResource>(name);
    }
}
