using Volo.Abp.Modularity;

namespace PharmacySystem;

/* Inherit from this class for your domain layer tests. */
public abstract class PharmacySystemDomainTestBase<TStartupModule> : PharmacySystemTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
