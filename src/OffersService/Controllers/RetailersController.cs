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
        //TODO : Handle the case when retailer is null (not found)
        return Ok(await _retailerService.GetRetailerWithOffersAsync(id));
    }
}
