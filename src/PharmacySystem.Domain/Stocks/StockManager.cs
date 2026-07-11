using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace PharmacySystem.Stocks;

// Domain service responsible for stock increase/decrease operations
public class StockManager : DomainService
{
    private readonly IRepository<Stock, Guid> _stockRepository;

    public StockManager(IRepository<Stock, Guid> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    // Increases stock for a medicine batch.
    // If the batch does not exist, creates a new stock row.
    public async Task IncreaseAsync(
        Guid medicineId,
        string batchNumber,
        DateTime? expiryDate,
        int quantity,
        decimal unitCost)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine is required.", nameof(medicineId));
        }

        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitCost < 0)
        {
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));
        }

        // Find the existing lot by medicine + batch + expiry. Expiry is part of
        // the lot identity: the same batch number delivered with a different
        // expiry date is a different physical lot and must not be merged.
        var existingStock = await _stockRepository.FirstOrDefaultAsync(
            x => x.MedicineId == medicineId &&
                 x.BatchNumber == batchNumber &&
                 x.ExpiryDate == expiryDate
        );

        if (existingStock == null)
        {
            // Create new stock row if this lot does not exist yet
            var stock = new Stock(
                GuidGenerator.Create(),
                medicineId,
                batchNumber,
                quantity,
                unitCost,
                expiryDate
            );

            await _stockRepository.InsertAsync(stock, autoSave: true);
            return;
        }

        // Increase quantity if the lot already exists
        existingStock.Increase(quantity);

        // Update latest unit cost for this lot
        existingStock.SetUnitCost(unitCost);

        await _stockRepository.UpdateAsync(existingStock, autoSave: true);
    }

    // Increases stock quantity for an existing lot WITHOUT changing its unit
    // cost. Use this when restoring previously-deducted stock (e.g. reversing a
    // sale on edit/delete): the sold quantity goes back to the lot it came from,
    // but the batch's purchase cost must be preserved for valuation/COGS. Never
    // feed a selling price into IncreaseAsync's unitCost for this purpose.
    public async Task IncreaseQuantityAsync(
        Guid medicineId,
        string batchNumber,
        DateTime? expiryDate,
        int quantity)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine is required.", nameof(medicineId));
        }

        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        var existingStock = await _stockRepository.FirstOrDefaultAsync(
            x => x.MedicineId == medicineId &&
                 x.BatchNumber == batchNumber &&
                 x.ExpiryDate == expiryDate
        );

        if (existingStock == null)
        {
            // The lot no longer exists (e.g. it was removed after depletion).
            // Recreate it with an unknown unit cost of 0 rather than fabricating
            // a cost from a selling price; the restored quantity is authoritative.
            var stock = new Stock(
                GuidGenerator.Create(),
                medicineId,
                batchNumber,
                quantity,
                0m,
                expiryDate
            );

            await _stockRepository.InsertAsync(stock, autoSave: true);
            return;
        }

        existingStock.Increase(quantity);

        await _stockRepository.UpdateAsync(existingStock, autoSave: true);
    }

    // Decreases stock for a medicine batch.
    public async Task DecreaseAsync(
        Guid medicineId,
        string batchNumber,
        DateTime? expiryDate,
        int quantity)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine is required.", nameof(medicineId));
        }

        if (string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new ArgumentException("Batch number is required.", nameof(batchNumber));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        // Find the lot by medicine + batch + expiry so stock is deducted from
        // the exact physical lot that was sold (matches how it was received).
        var existingStock = await _stockRepository.FirstOrDefaultAsync(
            x => x.MedicineId == medicineId &&
                 x.BatchNumber == batchNumber &&
                 x.ExpiryDate == expiryDate
        );

        if (existingStock == null)
        {
            throw new InvalidOperationException("Stock record not found.");
        }

        existingStock.Decrease(quantity);

        await _stockRepository.UpdateAsync(existingStock, autoSave: true);
    }
}