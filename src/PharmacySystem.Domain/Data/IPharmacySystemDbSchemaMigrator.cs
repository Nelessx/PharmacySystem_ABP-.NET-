using System.Threading.Tasks;

namespace PharmacySystem.Data;

public interface IPharmacySystemDbSchemaMigrator
{
    Task MigrateAsync();
}
