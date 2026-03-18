using Microsoft.EntityFrameworkCore;
using RentalApp.Application.Features.Pricing;
using RentalApp.Domain.Enums;
using RentalApp.Infrastructure.Persistence;
using RentalApp.Infrastructure.Persistence.Repositories;

namespace RentalApp.UnitTests;

public sealed class PricingRuleReadServiceTests
{
    [Fact]
    public async Task CalculateAsync_ReturnsWeekdayPrivateRate()
    {
        await using var dbContext = CreateDbContext();
        var service = new PricingRuleReadService(dbContext);

        var quote = await service.CalculateAsync(
            new PriceQuoteRequest(
                new DateTime(2026, 3, 18, 9, 0, 0, DateTimeKind.Utc),
                BookingMode.Private));

        Assert.Equal(280000m, quote.UnitPrice);
        Assert.Equal(280000m, quote.TotalPrice);
        Assert.False(quote.IsWeekend);
    }

    [Fact]
    public async Task CalculateAsync_AppliesWeekendMarkupForSharedBooking()
    {
        await using var dbContext = CreateDbContext();
        var service = new PricingRuleReadService(dbContext);

        var quote = await service.CalculateAsync(
            new PriceQuoteRequest(
                new DateTime(2026, 3, 21, 9, 0, 0, DateTimeKind.Utc),
                BookingMode.Shared,
                Quantity: 2));

        Assert.Equal(54000m, quote.UnitPrice);
        Assert.Equal(108000m, quote.TotalPrice);
        Assert.True(quote.IsWeekend);
    }

    private static RentalAppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RentalAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var dbContext = new RentalAppDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
