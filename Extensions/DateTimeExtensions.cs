namespace TF2PugBot.Extensions;

public static class DateTimeExtensions
{
    public static int HoursFromNow (this DateTime dt)
    {
        return (dt - DateTime.Now).Hours;
    }
}