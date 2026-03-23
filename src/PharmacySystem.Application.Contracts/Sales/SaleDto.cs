using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Sales;

// Output DTO for Sale (header + items)
public class SaleDto : FullAuditedEntityDto<Guid>
{
    // Sale number shown to users
    public string SaleNumber { get; set; } = string.Empty;

    // Optional customer reference
    public Guid? CustomerId { get; set; }

    // Optional readable customer name for UI
    public string? CustomerName { get; set; }

    // Date of sale
    public DateTime SaleDate { get; set; }

    // Optional notes
    public string? Notes { get; set; }

    // Total before discount
    public decimal TotalAmount { get; set; }

    // Discount amount
    public decimal DiscountAmount { get; set; }

    // Final total
    public decimal NetAmount { get; set; }

    // Sale item lines
    public List<SaleItemDto> Items { get; set; } = new();
}