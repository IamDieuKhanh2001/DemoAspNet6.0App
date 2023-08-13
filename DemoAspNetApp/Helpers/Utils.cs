namespace DemoAspNetApp.Helpers
{
    public class Utils
    {
        public static DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            return DateTimeOffset.FromUnixTimeSeconds(utcExpireDate).UtcDateTime;
        }
    }
}
