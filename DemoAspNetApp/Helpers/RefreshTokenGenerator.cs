using System.Security.Cryptography;
namespace DemoAspNetApp.Helpers
{
    public class RefreshTokenGenerator
    {
        public static string Generate()
        {
            var random = new Byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            return Convert.ToBase64String(random);
        }
    }
}
