namespace OffersService.Services;

public class SystemDateTimeProvider: IDateTimeProvider
{
    public DateTime UtcNow { get; set => UtcNow = DateTime.UtcNow; }
}
