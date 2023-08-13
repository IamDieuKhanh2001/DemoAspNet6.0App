using DemoAspNetApp.Data;

namespace DemoAspNetApp.Models
{
    public class SignInResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public UserVM UserInfo { get; set; }
    }
}
