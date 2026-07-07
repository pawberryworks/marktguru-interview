using Microsoft.AspNetCore.Mvc;
using OffersService.DTOs;
using OffersService.Services;

namespace OffersService.Controllers;

[ApiController]
[Route("api/retailers")]
public class RetailersController : ControllerBase
{
    private readonly IRetailerService _retailerService;

    public RetailersController(IRetailerService retailerService)
    {
        _retailerService = retailerService;
    }

    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<RetailerWithOffersDto>> GetStatsById(int id)
    {
        try
        {
            var dto = await _retailerService.GetRetailerWithOffersAsync(id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            // Return exception message for test visibility (kept concise)
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] DTOs.RetailerCreateDto dto)
    {
        // Model validation handled by [ApiController] — invalid model state results in 400
        try
        {
            var id = await _retailerService.CreateRetailerAsync(dto);
            // Location header pointing to the new resource
            return Created($"/api/retailers/{id}", new { id });
        }
        catch (DuplicateRetailerException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
