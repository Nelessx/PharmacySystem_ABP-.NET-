using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace PharmacySystem.Sales;

public interface ISaleRepository : IRepository<Sale, Guid>
{
    Task<List<TopSellingMedicineQueryResult>> GetTopSellingMedicinesAsync(int count);
}

public class TopSellingMedicineQueryResult
{
    public Guid MedicineId { get; set; }
    public int TotalQuantitySold { get; set; }
}