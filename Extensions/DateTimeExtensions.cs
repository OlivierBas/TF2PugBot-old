namespace TF2PugBot.Extensions;

public static class DateTimeExtensions
{
    public static int HoursFromNow (this DateTime dt)
    {
        return (DateTime.Now - dt).Hours;
    }
    public static int MinutesFromNow (this DateTime dt)
    {
        return (DateTime.Now - dt).Minutes;
    }
}