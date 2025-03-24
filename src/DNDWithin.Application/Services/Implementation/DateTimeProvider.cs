namespace DNDWithin.Application.Services.Implementation;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;
}