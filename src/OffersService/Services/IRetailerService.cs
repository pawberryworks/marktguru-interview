using OffersService.DTOs;

namespace OffersService.Services;

public interface IRetailerService
{
    Task<RetailerWithOffersDto> GetRetailerWithOffersAsync(int id);
    Task<int> CreateRetailerAsync(DTOs.RetailerCreateDto dto);
}
