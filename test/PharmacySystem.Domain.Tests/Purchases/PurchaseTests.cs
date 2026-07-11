using System;
using PharmacySystem.Purchases;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace PharmacySystem.Purchases;

// Pure domain unit tests for the Purchase aggregate (no database).
public class PurchaseTests
{
    private static Purchase CreatePurchase(decimal discount = 0)
        => new(Guid.NewGuid(), "PUR-1", Guid.NewGuid(), DateTime.Today, invoiceNumber: null, notes: null, discountAmount: discount);

    [Fact]
    public void AddItem_updates_totals()
    {
        var purchase = CreatePurchase();
        purchase.AddItem(Guid.NewGuid(), 10, 3m, "B1", DateTime.Today.AddYears(1));

        purchase.TotalAmount.ShouldBe(30m);
        purchase.NetAmount.ShouldBe(30m);
    }

    [Fact]
    public void Discount_reduces_net_amount()
    {
        var purchase = CreatePurchase(discount: 5m);
        purchase.AddItem(Guid.NewGuid(), 10, 3m, "B1", null);

        purchase.NetAmount.ShouldBe(25m);
    }

    [Fact]
    public void EnsureValid_throws_when_discount_exceeds_total()
    {
        var purchase = CreatePurchase(discount: 100m);
        purchase.AddItem(Guid.NewGuid(), 1, 30m, "B1", null);

        Should.Throw<BusinessException>(() => purchase.EnsureValid())
            .Code.ShouldBe(PharmacySystemDomainErrorCodes.DiscountExceedsTotal);
    }

    [Fact]
    public void Default_purchase_date_is_rejected()
    {
        Should.Throw<ArgumentException>(() =>
            new Purchase(Guid.NewGuid(), "PUR-1", Guid.NewGuid(), default, null, null, 0));
    }

    [Fact]
    public void Empty_supplier_is_rejected()
    {
        Should.Throw<ArgumentException>(() =>
            new Purchase(Guid.NewGuid(), "PUR-1", Guid.Empty, DateTime.Today, null, null, 0));
    }
}
