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
        var retailer = await _context.Retailers.FindAsync(id);
        //TODO : Handle the case when retailer is null (not found)
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
