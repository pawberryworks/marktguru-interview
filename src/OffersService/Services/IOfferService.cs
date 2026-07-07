using OffersService.DTOs;

namespace OffersService.Services;

public interface IOfferService
{
    Task<IEnumerable<OfferDto>> GetActiveOffersAsync();
    Task<OfferDto?> GetByIdAsync(int id);
    Task<PaginatedOffersResponse> GetPaginatedOffersAsync(int page, int pageSize, int? productId, int? retailerId, Models.OfferFilterStatus status);
}
