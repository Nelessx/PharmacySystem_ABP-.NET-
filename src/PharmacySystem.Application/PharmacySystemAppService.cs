using PharmacySystem.Localization;
using Volo.Abp.Application.Services;

namespace PharmacySystem;

/* Inherit your application services from this class.
 */
public abstract class PharmacySystemAppService : ApplicationService
{
    protected PharmacySystemAppService()
    {
        LocalizationResource = typeof(PharmacySystemResource);
    }
}
