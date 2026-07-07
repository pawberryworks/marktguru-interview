using Microsoft.AspNetCore.Mvc;
using OffersService.DTOs;
using OffersService.Services;

namespace OffersService.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;
    private readonly IOfferImportService _importService;

    public OffersController(IOfferService offerService, IOfferImportService importService)
    {
        _offerService  = offerService;
        _importService = importService;
    }

    [HttpGet()]
    public async Task<ActionResult<PaginatedOffersResponse>> GetPaginatedOffers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? productId = null,
        [FromQuery] int? retailerId = null,
        [FromQuery] string status = "active")
    {
        if (page < 1)
            return BadRequest("page must be >= 1");

        if (pageSize < 1)
            return BadRequest("pageSize must be >= 1");

        if (pageSize > 100)
            return BadRequest("pageSize cannot exceed 100.");

        var s = (status ?? string.Empty).ToLowerInvariant();
        if (s != "active" && s != "expired" && s != "all")
            return BadRequest("status must be one of: active, expired, all");

        var offers = await _offerService.GetPaginatedOffersAsync(page, pageSize, productId, retailerId, s);
        return Ok(offers);
    }

    [HttpGet("active")]
    public ActionResult<IEnumerable<OfferDto>> GetActiveOffers()
    {
        var offers = _offerService.GetActiveOffersAsync().Result;
        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfferDto>> GetById(int id)
    {
        var offer = await _offerService.GetByIdAsync(id);
        if (offer is null)
            return NotFound();
        return Ok(offer);
    }

    [HttpPost("import")]
    public async Task<ActionResult<int>> Import([FromBody] ImportRequest request)
    {
        if (request?.Rows is null)
            return BadRequest("Request rows cannot be null.");

        try
        {
            var count = await _importService.ImportAsync(request.Rows);
            return Accepted(count);
        }
        catch (Exception ex)
        {
            // Let the client know something went wrong — tests expect 500 when FK violation occurs
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
