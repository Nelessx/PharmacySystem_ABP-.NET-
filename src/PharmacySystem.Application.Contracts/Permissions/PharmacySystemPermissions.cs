namespace PharmacySystem.Permissions;

public static class PharmacySystemPermissions
{
    public const string GroupName = "PharmacySystem";



    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public static class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Medicines
    {
        public const string Default = GroupName + ".Medicines";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Suppliers
    {
        public const string Default = GroupName + ".Suppliers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Purchases
    {
        public const string Default = GroupName + ".Purchases";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Sales
    {
        public const string Default = GroupName + ".Sales";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Stock
    {
        public const string Default = GroupName + ".Stock";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
    }
}
