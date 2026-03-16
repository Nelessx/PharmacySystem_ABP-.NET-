using PharmacySystem.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace PharmacySystem.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class PharmacySystemController : AbpControllerBase
{
    protected PharmacySystemController()
    {
        LocalizationResource = typeof(PharmacySystemResource);
    }
}
