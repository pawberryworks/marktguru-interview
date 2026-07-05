namespace OffersService.Services;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}
