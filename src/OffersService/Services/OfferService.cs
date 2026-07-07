using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class OfferService : IOfferService
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public OfferService(AppDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IEnumerable<OfferDto>> GetActiveOffersAsync()
    {
        var now = _dateTimeProvider.UtcNow;
        var offers = await _context.Offers
            .Include(o => o.Product)
            .Include(o => o.Retailer)
            .Where(o => o.ValidTo >= now && o.Status == OfferStatus.Active)
            .ToListAsync();

        return offers.Select(o => new OfferDto(o));
    }

    public async Task<PaginatedOffersResponse> GetPaginatedOffersAsync(int page, int pageSize, int? productId, int? retailerId, OfferFilterStatus status)
    {
        ValidatePaginationRequest(page, pageSize);

        var now = _dateTimeProvider.UtcNow;
        var query = _context.Offers
            .Include(o => o.Product)
            .Include(o => o.Retailer)
            .AsQueryable();
        // Status filter
        switch (status)
        {
            case OfferFilterStatus.Active:
                query = query.Where(o => o.ValidTo >= now && o.Status == OfferStatus.Active);
                break;
            case OfferFilterStatus.Expired:
                query = query.Where(o => o.ValidTo < now || o.Status == OfferStatus.Expired);
                break;
            case OfferFilterStatus.All:
            default:
                // no filter
                break;
        }

        // Optional filters
        if (productId.HasValue)
            query = query.Where(o => o.ProductId == productId.Value);

        if (retailerId.HasValue)
            query = query.Where(o => o.RetailerId == retailerId.Value);

        // Compute total before pagination
        var total = await query.CountAsync();

        // Order and paginate
        var offers = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = offers.Select(o => new OfferDto(o)).ToList();

        return new PaginatedOffersResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OfferDto?> GetByIdAsync(int id)
    {
        var offer = await _context.Offers
            .Include(o => o.Product)
            .Include(o => o.Retailer)
            .FirstOrDefaultAsync(o => o.Id == id);

        return offer is null ? null : new OfferDto(offer);
    }

    private void ValidatePaginationRequest(int page, int pageSize)
    {
        // Validate paging inputs here so controller can simply translate validation errors to 400
        if (page < 1)
            throw new ArgumentException("page must be >= 1");

        if (pageSize < 1)
            throw new ArgumentException("pageSize must be >= 1");

        if (pageSize > 100)
            throw new ArgumentException("pageSize cannot exceed 100.");
    }
}
