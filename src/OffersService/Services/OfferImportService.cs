using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class OfferImportService : IOfferImportService
{
    private readonly AppDbContext _context;

    public OfferImportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> ImportAsync(IEnumerable<OfferRow> rows)
    {
        foreach (var row in rows)
        {
            if (!await productExistAsync(row.ProductId))
            {
                throw new Exception(string.Format("Product with Id {0} cannot be null", row.ProductId));
            }
            await _context.Offers.AddAsync(new Offer
            {
                ProductId = row.ProductId,
                Price     = row.Price,
                ValidFrom = row.ValidFrom,
                ValidTo   = row.ValidTo,
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
        return rows.Count();
    }

    async Task<bool> productExistAsync(int productId)
    {
        return await _context.Products.AnyAsync(o => o.Id == productId);
    }
}