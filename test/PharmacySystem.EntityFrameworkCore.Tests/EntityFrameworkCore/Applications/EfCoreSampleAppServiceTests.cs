using PharmacySystem.Samples;
using Xunit;

namespace PharmacySystem.EntityFrameworkCore.Applications;

[Collection(PharmacySystemTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<PharmacySystemEntityFrameworkCoreTestModule>
{

}
