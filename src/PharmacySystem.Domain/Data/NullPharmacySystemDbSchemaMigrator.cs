using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace PharmacySystem.Data;

/* This is used if database provider does't define
 * IPharmacySystemDbSchemaMigrator implementation.
 */
public class NullPharmacySystemDbSchemaMigrator : IPharmacySystemDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
