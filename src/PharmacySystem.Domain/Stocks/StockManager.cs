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

        // Find existing stock by medicine + batch
        var existingStock = await _stockRepository.FirstOrDefaultAsync(
            x => x.MedicineId == medicineId &&
                 x.BatchNumber == batchNumber
        );

        if (existingStock == null)
        {
            // Create new stock row if batch does not exist
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

        // Increase quantity if batch already exists
        existingStock.Increase(quantity);

        // Update latest unit cost
        existingStock.SetUnitCost(unitCost);

        // If existing record has no expiry yet, keep/update it
        if (!existingStock.ExpiryDate.HasValue && expiryDate.HasValue)
        {
            existingStock.SetExpiryDate(expiryDate);
        }

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

        // Find stock by medicine + batch only
        var existingStock = await _stockRepository.FirstOrDefaultAsync(
            x => x.MedicineId == medicineId &&
                 x.BatchNumber == batchNumber
        );

        if (existingStock == null)
        {
            throw new InvalidOperationException("Stock record not found.");
        }

        existingStock.Decrease(quantity);

        await _stockRepository.UpdateAsync(existingStock, autoSave: true);
    }
}