using PharmacySystem.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace PharmacySystem.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(PharmacySystemEntityFrameworkCoreModule),
    typeof(PharmacySystemApplicationContractsModule)
)]
public class PharmacySystemDbMigratorModule : AbpModule
{
}
