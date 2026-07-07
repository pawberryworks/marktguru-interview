using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.DTOs;

public class PaginatedOffersResponse
{
    public IEnumerable<OfferDto> Items { get; set; } = Enumerable.Empty<OfferDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}