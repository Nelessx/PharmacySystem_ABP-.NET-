using System;
using PharmacySystem.Stocks;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace PharmacySystem.Stocks;

// Pure domain unit tests for the Stock entity invariants (no database).
public class StockTests
{
    private static Stock CreateStock(int quantity = 100, decimal unitCost = 10m)
        => new(Guid.NewGuid(), Guid.NewGuid(), "B1", quantity, unitCost, DateTime.Today.AddYears(1));

    [Fact]
    public void Increase_adds_quantity()
    {
        var stock = CreateStock(100);
        stock.Increase(50);
        stock.Quantity.ShouldBe(150);
    }

    [Fact]
    public void Decrease_reduces_quantity()
    {
        var stock = CreateStock(100);
        stock.Decrease(30);
        stock.Quantity.ShouldBe(70);
    }

    [Fact]
    public void Decrease_to_exactly_zero_is_allowed()
    {
        var stock = CreateStock(10);
        stock.Decrease(10);
        stock.Quantity.ShouldBe(0);
    }

    [Fact]
    public void Decrease_more_than_available_throws_insufficient_stock()
    {
        var stock = CreateStock(10);
        var ex = Should.Throw<BusinessException>(() => stock.Decrease(20));
        ex.Code.ShouldBe(PharmacySystemDomainErrorCodes.InsufficientStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Increase_or_Decrease_by_non_positive_throws(int quantity)
    {
        var stock = CreateStock(10);
        Should.Throw<ArgumentException>(() => stock.Increase(quantity));
        Should.Throw<ArgumentException>(() => stock.Decrease(quantity));
    }

    [Fact]
    public void Cannot_create_with_negative_quantity()
    {
        Should.Throw<ArgumentException>(() =>
            new Stock(Guid.NewGuid(), Guid.NewGuid(), "B1", -1, 10m, null));
    }

    [Fact]
    public void Cannot_create_with_negative_unit_cost()
    {
        Should.Throw<ArgumentException>(() =>
            new Stock(Guid.NewGuid(), Guid.NewGuid(), "B1", 10, -1m, null));
    }
}
