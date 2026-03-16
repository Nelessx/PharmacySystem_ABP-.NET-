using Volo.Abp.Modularity;

namespace PharmacySystem;

[DependsOn(
    typeof(PharmacySystemApplicationModule),
    typeof(PharmacySystemDomainTestModule)
)]
public class PharmacySystemApplicationTestModule : AbpModule
{

}
