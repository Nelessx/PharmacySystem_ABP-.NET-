using PharmacySystem.Samples;
using Xunit;

namespace PharmacySystem.EntityFrameworkCore.Domains;

[Collection(PharmacySystemTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<PharmacySystemEntityFrameworkCoreTestModule>
{

}
