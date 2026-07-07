using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class RetailerService: IRetailerService
{
    private readonly AppDbContext _context;

    public RetailerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RetailerWithOffersDto> GetRetailerWithOffersAsync(int id)
    {
        var now = DateTime.UtcNow;

        // Single-query projection: filter by id and active flag, then compute aggregates and most recent active offer
        if (_context.Database.IsRelational())
        {
            var projected = await _context.Retailers
                .Where(r => r.Id == id && r.IsActive)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Country,
                    r.IsActive,
                    ActiveOfferCount = r.Offers.Count(o => o.Status == OfferStatus.Active && o.ValidTo >= now),
                    AvgPrice = r.Offers.Where(o => o.Status == OfferStatus.Active && o.ValidTo >= now).Select(o => (decimal?)o.Price).Average() ?? 0m,
                    LowestPrice = r.Offers.Where(o => o.Status == OfferStatus.Active && o.ValidTo >= now).Select(o => (decimal?)o.Price).Min(),
                    MostRecent = r.Offers.Where(o => o.Status == OfferStatus.Active && o.ValidTo >= now)
                        .OrderByDescending(o => o.ValidFrom)
                        .Select(o => new OfferSummaryDto(o.Id, o.Price, o.ValidFrom, o.ValidTo))
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (projected is null)
                throw new KeyNotFoundException($"Retailer with id {id} not found or is inactive.");

            // Map decimals to floats for the DTO (matches previous shape)
            var avg = (float)projected.AvgPrice;
            var lowest = projected.LowestPrice == null ? float.MaxValue : (float)projected.LowestPrice.Value;

            return new RetailerWithOffersDto(
                projected.Id,
                projected.Name,
                projected.Country,
                projected.IsActive,
                projected.ActiveOfferCount,
                avg,
                lowest,
                projected.MostRecent);
        }

        // Fallback for non-relational providers (InMemory used in tests)
        var retailer = await _context.Retailers.FindAsync(id);
        if (retailer is null || !retailer.IsActive)
            throw new KeyNotFoundException($"Retailer with id {id} not found or is inactive.");

        retailer.Offers = (ICollection<Offer>)await _context.Offers
            .Where(o => o.RetailerId == retailer.Id)
            .ToListAsync();

        return new RetailerWithOffersDto(retailer);
    }

    public async Task<int> CreateRetailerAsync(DTOs.RetailerCreateDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var name = dto.Name?.Trim() ?? string.Empty;
        var country = dto.Country?.Trim().ToUpperInvariant() ?? string.Empty;

        // Check for duplicate name (case-insensitive)
        var exists = await _context.Retailers.AnyAsync(r => r.Name.ToLower() == name.ToLower());
        if (exists)
            throw new DuplicateRetailerException($"A retailer with the name '{name}' already exists.");

        var retailer = new Models.Retailer
        {
            Name = name,
            Country = country,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Retailers.Add(retailer);
        await _context.SaveChangesAsync();

        return retailer.Id;
    }
}
