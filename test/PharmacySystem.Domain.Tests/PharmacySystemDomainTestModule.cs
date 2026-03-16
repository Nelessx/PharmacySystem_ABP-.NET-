using Volo.Abp.Modularity;

namespace PharmacySystem;

[DependsOn(
    typeof(PharmacySystemDomainModule),
    typeof(PharmacySystemTestBaseModule)
)]
public class PharmacySystemDomainTestModule : AbpModule
{

}
