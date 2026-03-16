using Microsoft.Extensions.Localization;
using PharmacySystem.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace PharmacySystem;

[Dependency(ReplaceServices = true)]
public class PharmacySystemBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<PharmacySystemResource> _localizer;

    public PharmacySystemBrandingProvider(IStringLocalizer<PharmacySystemResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
