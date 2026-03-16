using Volo.Abp.Modularity;

namespace PharmacySystem;

public abstract class PharmacySystemApplicationTestBase<TStartupModule> : PharmacySystemTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
