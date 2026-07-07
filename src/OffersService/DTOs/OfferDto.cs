using OffersService.Models;

namespace OffersService.DTOs;

public class OfferDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int? RetailerId { get; set; }

    public string RetailerName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public string Status { get; set; } = string.Empty;

    // Parameterless constructor required for System.Text.Json deserialization in tests
    public OfferDto() { }

    public OfferDto(Offer offer)
    {
        Id = offer.Id;
        ProductId = offer.ProductId;
        ProductName = offer.Product?.Name ?? string.Empty;
        RetailerId = offer.RetailerId;
        RetailerName = offer.Retailer?.Name ?? string.Empty;
        Price = offer.Price;
        ValidFrom = offer.ValidFrom;
        ValidTo = offer.ValidTo;
        Status = offer.Status.ToString();
    }
}
