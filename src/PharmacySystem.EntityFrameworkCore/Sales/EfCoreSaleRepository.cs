using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Sales;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace PharmacySystem.EntityFrameworkCore.Sales;

public class EfCoreSaleRepository
    : EfCoreRepository<PharmacySystemDbContext, Sale, Guid>, ISaleRepository
{
    public EfCoreSaleRepository(IDbContextProvider<PharmacySystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<TopSellingMedicineQueryResult>> GetTopSellingMedicinesAsync(int count)
    {
        var dbContext = await GetDbContextAsync();

        var result = await dbContext
            .Set<Sale>()
            .SelectMany(x => EF.Property<List<SaleItem>>(x, "_items"))
            .GroupBy(x => x.MedicineId)
            .Select(g => new TopSellingMedicineQueryResult
            {
                MedicineId = g.Key,
                TotalQuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(count)
            .ToListAsync();

        return result;
    }
}