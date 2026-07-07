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
}
