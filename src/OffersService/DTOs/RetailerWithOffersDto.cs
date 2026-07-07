using OffersService.Models;

namespace OffersService.DTOs;

public record RetailerWithOffersDto(
    int Id,
    string Name,
    string Country,
    bool IsActive,
    int ActiveOfferCount,
    float AvgPrice,
    float LowestPrice,
    OfferSummaryDto MostRecentActiveOffer)
{
    public RetailerWithOffersDto(Retailer retailer) : this(
        retailer.Id,
        retailer.Name,
        retailer.Country,
        retailer.IsActive,
        retailer.Offers.Count(o => o.Status == OfferStatus.Active),
        retailer.Offers.Where(o => o.Status == OfferStatus.Active).Select(o => (float)o.Price).DefaultIfEmpty(0).Average(),
        retailer.Offers.Where(o => o.Status == OfferStatus.Active).Select(o => (float)o.Price).DefaultIfEmpty(float.MaxValue).Min(),
        retailer.Offers.Where(o => o.Status == OfferStatus.Active).OrderByDescending(o => o.ValidFrom).Select(o => new OfferSummaryDto(o.Id, o.Price, o.ValidFrom, o.ValidTo)).FirstOrDefault()
        )
    {
    }
}