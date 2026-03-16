using Xunit;

namespace PharmacySystem.EntityFrameworkCore;

[CollectionDefinition(PharmacySystemTestConsts.CollectionDefinitionName)]
public class PharmacySystemEntityFrameworkCoreCollection : ICollectionFixture<PharmacySystemEntityFrameworkCoreFixture>
{

}
