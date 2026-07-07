using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OffersService.Data;
using OffersService.Jobs;
using OffersService.Models;
using OffersService.Services;
using Xunit;

namespace OffersService.Tests.Unit;

public class OfferExpiryJobTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_ExpiresOnlyMatchingOffers_AndCountMatches()
    {
        // Arrange
        var dbName = "OfferExpiryTest_" + Guid.NewGuid();
        await using var context = CreateContext(dbName);

        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var provider = new FakeDateTimeProvider(now);

        // Offers:
        // 1: ValidTo < now, Status = Active => should be expired
        // 2: ValidTo < now, Status = Expired => should not be changed
        // 3: ValidTo >= now, Status = Active => should not be changed
        // 4: ValidTo < now, Status = Pending => should be expired

        context.Offers.AddRange(
            new Offer { Id = 1, ProductId = 1, ValidFrom = now.AddDays(-10), ValidTo = now.AddMinutes(-1), Status = OfferStatus.Active, CreatedAt = now },
            new Offer { Id = 2, ProductId = 1, ValidFrom = now.AddDays(-10), ValidTo = now.AddMinutes(-2), Status = OfferStatus.Expired, CreatedAt = now },
            new Offer { Id = 3, ProductId = 1, ValidFrom = now, ValidTo = now.AddHours(1), Status = OfferStatus.Active, CreatedAt = now },
            new Offer { Id = 4, ProductId = 1, ValidFrom = now.AddDays(-5), ValidTo = now.AddDays(-1), Status = OfferStatus.Pending, CreatedAt = now }
        );
        await context.SaveChangesAsync();

        var job = new OfferExpiryJob(context, provider, NullLogger<OfferExpiryJob>.Instance);

        // Compute expected: offers with ValidTo < now AND Status != Expired
        var preList = await context.Offers.AsNoTracking().ToListAsync();
        var expected = preList.Count(o => o.ValidTo < now && o.Status != OfferStatus.Expired);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var postList = await context.Offers.AsNoTracking().ToListAsync();

        // Count how many changed from non-Expired to Expired
        var changed = postList.Count(o => o.Status == OfferStatus.Expired && preList.Single(p => p.Id == o.Id).Status != OfferStatus.Expired);

        Assert.Equal(expected, changed);

        // Also assert that offers that shouldn't be expired remain with their original status
        Assert.Equal(OfferStatus.Expired, postList.Single(o => o.Id == 1).Status);
        Assert.Equal(OfferStatus.Expired, postList.Single(o => o.Id == 2).Status); // already expired
        Assert.Equal(OfferStatus.Active, postList.Single(o => o.Id == 3).Status);
        Assert.Equal(OfferStatus.Expired, postList.Single(o => o.Id == 4).Status);
    }
}
