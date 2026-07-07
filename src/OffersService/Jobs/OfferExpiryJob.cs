using Hangfire;
using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;
using OffersService.Services;

namespace OffersService.Jobs;

public class OfferExpiryJob
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<OfferExpiryJob> _logger;

    public OfferExpiryJob(AppDbContext context, IDateTimeProvider dateTimeProvider, ILogger<OfferExpiryJob> logger)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    // Hangfire will call this. Accepts a CancellationToken and propagates it to EF Core.
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        // Build the query of offers that need expiring
        var query = _context.Offers
            .Where(o => o.ValidTo < now && o.Status != OfferStatus.Expired);

        int updated;
        try
        {
            // Try to execute a single bulk UPDATE (supported by relational providers)
            updated = await query.ExecuteUpdateAsync(
                setters => setters.SetProperty(o => o.Status, OfferStatus.Expired),
                cancellationToken: cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message?.Contains("not supported") == true || ex.Message?.Contains("ExecuteUpdate") == true)
        {
            // Fallback for providers (like InMemory) that don't support ExecuteUpdateAsync:
            // load affected rows, update in memory and save changes. This is only used in tests.
            var ids = await query.Select(o => o.Id).ToListAsync(cancellationToken);
            if (ids.Count == 0)
            {
                updated = 0;
            }
            else
            {
                var offers = await _context.Offers.Where(o => ids.Contains(o.Id)).ToListAsync(cancellationToken);
                foreach (var o in offers)
                    o.Status = OfferStatus.Expired;

                updated = await _context.SaveChangesAsync(cancellationToken);
            }
        }

        _logger.LogInformation("Expired {Count} offers at {Timestamp}", updated, now.ToString("o"));
    }
}
