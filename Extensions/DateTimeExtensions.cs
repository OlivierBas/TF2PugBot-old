namespace TF2PugBot.Extensions;

public static class DateTimeExtensions
{
    public static int HoursFromNow (this DateTime dt)
    {
        return (int)(DateTime.Now - dt).TotalHours;
    }
    public static int MinutesFromNow (this DateTime dt)
    {
        return (int)(DateTime.Now - dt).TotalMinutes;
    }
}