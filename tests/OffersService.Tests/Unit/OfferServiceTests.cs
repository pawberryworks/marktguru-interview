using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;
using OffersService.Services;
using Xunit;

namespace OffersService.Tests.Unit;

public class FakeDateTimeProvider : IDateTimeProvider { public DateTime UtcNow { get; set; } public FakeDateTimeProvider(DateTime utcNow) => UtcNow = utcNow; }

public class OfferServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("OfferServiceTest_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetActiveOffersAsync_ReturnsOnlyOffersWithFutureValidTo()
    {
        // Arrange
        await using var context = CreateContext();
        var fakeDateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var now = fakeDateTimeProvider.UtcNow;

        var product = new Product
        {
            Id        = 1,
            Name      = "Test Product",
            Category  = "Test",
            CreatedAt = now
        };
        context.Products.Add(product);

        context.Offers.AddRange(
            new Offer
            {
                Id        = 1,
                ProductId = 1,
                Price     = 1.00m,
                ValidFrom = now.AddDays(-10),
                ValidTo   = now.AddDays(-1),   // expired
                Status    = OfferStatus.Active,
                CreatedAt = now
            },
            new Offer
            {
                Id        = 2,
                ProductId = 1,
                Price     = 2.00m,
                ValidFrom = now.AddDays(-1),
                ValidTo   = now.AddDays(10),   // valid
                Status    = OfferStatus.Active,
                CreatedAt = now
            }
        );
        await context.SaveChangesAsync();

        var service = new OfferService(context, fakeDateTimeProvider);

        // Act
        var result = await service.GetActiveOffersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result.First().Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOfferDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var fakeDateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var service = new OfferService(context, fakeDateTimeProvider);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveOffersAsync_ReturnsAllSeedOffers()
    {
        // Arrange
        await using var context = CreateContext();
        var fakeDateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        context.Products.AddRange(
            new Product { Id = 1, Name = "Whole Milk 1L",        Category = "Dairy",     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Sourdough Bread 500g", Category = "Bakery",    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Orange Juice 1L",      Category = "Beverages", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        context.Offers.AddRange(
            new Offer { Id = 1, ProductId = 1, Price = 0.99m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 2, ProductId = 2, Price = 1.49m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2024, 1,  2, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 3, ProductId = 3, Price = 1.29m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
        await context.SaveChangesAsync();

        var service = new OfferService(context, fakeDateTimeProvider);

        // Act
        var result = await service.GetActiveOffersAsync();

        // Assert — expects 3 but service correctly returns 2 (Offer 2 has ValidTo in the past)
        Assert.Equal(3, result.Count());
    }
}
