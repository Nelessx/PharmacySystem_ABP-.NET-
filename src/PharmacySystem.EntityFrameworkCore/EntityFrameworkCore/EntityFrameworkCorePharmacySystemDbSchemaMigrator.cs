using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmacySystem.Data;
using Volo.Abp.DependencyInjection;

namespace PharmacySystem.EntityFrameworkCore;

public class EntityFrameworkCorePharmacySystemDbSchemaMigrator
    : IPharmacySystemDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCorePharmacySystemDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the PharmacySystemDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<PharmacySystemDbContext>()
            .Database
            .MigrateAsync();
    }
}
