namespace API.Common.Providers;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;
}
