using System;
using System.Linq;
using PharmacySystem.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace PharmacySystem.Sales;

// Pure domain unit tests for the Sale aggregate (no database).
public class SaleTests
{
    private static Sale CreateSale(decimal discount = 0)
        => new(Guid.NewGuid(), "SAL-1", DateTime.Today, customerId: null, notes: null, discountAmount: discount);

    [Fact]
    public void AddItem_updates_totals()
    {
        var sale = CreateSale();
        sale.AddItem(Guid.NewGuid(), 2, 50m, "B1", DateTime.Today.AddYears(1));

        sale.TotalAmount.ShouldBe(100m);
        sale.NetAmount.ShouldBe(100m);
        sale.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void Discount_reduces_net_amount()
    {
        var sale = CreateSale(discount: 20m);
        sale.AddItem(Guid.NewGuid(), 1, 100m, "B1", null);

        sale.NetAmount.ShouldBe(80m);
    }

    [Fact]
    public void EnsureValid_throws_when_discount_exceeds_total()
    {
        var sale = CreateSale(discount: 200m);
        sale.AddItem(Guid.NewGuid(), 1, 100m, "B1", null);

        Should.Throw<BusinessException>(() => sale.EnsureValid())
            .Code.ShouldBe(PharmacySystemDomainErrorCodes.DiscountExceedsTotal);
    }

    [Fact]
    public void EnsureValid_passes_when_discount_within_total()
    {
        var sale = CreateSale(discount: 50m);
        sale.AddItem(Guid.NewGuid(), 1, 100m, "B1", null);

        Should.NotThrow(() => sale.EnsureValid());
    }

    [Fact]
    public void RemoveItem_recalculates_totals()
    {
        var sale = CreateSale();
        sale.AddItem(Guid.NewGuid(), 1, 100m, "B1", null);
        var itemId = sale.Items.First().Id;

        sale.RemoveItem(itemId);

        sale.TotalAmount.ShouldBe(0m);
        sale.Items.ShouldBeEmpty();
    }

    [Fact]
    public void ClearItems_empties_the_sale()
    {
        var sale = CreateSale();
        sale.AddItem(Guid.NewGuid(), 1, 100m, "B1", null);
        sale.AddItem(Guid.NewGuid(), 1, 40m, "B2", null);

        sale.ClearItems();

        sale.TotalAmount.ShouldBe(0m);
        sale.Items.ShouldBeEmpty();
    }

    [Fact]
    public void Default_sale_date_is_rejected()
    {
        Should.Throw<ArgumentException>(() =>
            new Sale(Guid.NewGuid(), "SAL-1", default, null, null, 0));
    }

    [Fact]
    public void Negative_discount_is_rejected()
    {
        Should.Throw<ArgumentException>(() => CreateSale(discount: -10m));
    }

    [Fact]
    public void Empty_sale_number_is_rejected()
    {
        Should.Throw<ArgumentException>(() =>
            new Sale(Guid.NewGuid(), "  ", DateTime.Today, null, null, 0));
    }
}
