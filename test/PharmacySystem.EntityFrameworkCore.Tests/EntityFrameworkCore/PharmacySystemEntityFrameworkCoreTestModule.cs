using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace PharmacySystem.EntityFrameworkCore;

[DependsOn(
    typeof(PharmacySystemApplicationTestModule),
    typeof(PharmacySystemEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class PharmacySystemEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);

    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        // Use a shared-cache in-memory database identified by a unique name, and
        // keep one connection open for the lifetime of the test run so the
        // database is not discarded. Unlike a single shared connection object,
        // this lets each DbContext open its OWN connection to the same in-memory
        // database; Microsoft.Data.Sqlite then auto-retries on SQLITE_BUSY/LOCKED
        // instead of failing, which eliminates the intermittent "database is
        // locked" errors when ABP's data seeders issue concurrent commands.
        var connectionString = $"Data Source=PharmacySystemTests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        _sqliteConnection = new SqliteConnection(connectionString);
        _sqliteConnection.Open();

        CreateDatabaseSchema(connectionString);

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(context =>
            {
                context.DbContextOptions.UseSqlite(connectionString);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }

    private static void CreateDatabaseSchema(string connectionString)
    {
        var options = new DbContextOptionsBuilder<PharmacySystemDbContext>()
            .UseSqlite(connectionString)
            .Options;

        using var context = new PharmacySystemDbContext(options);
        context.GetService<IRelationalDatabaseCreator>().CreateTables();
    }
}
