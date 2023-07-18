using DemoAspNetApp.Data;

namespace DemoAspNetApp.Models
{
    public class SignInResponse
    {
        public string JwtToken { get; set; }
        public UserVM User { get; set; }
    }
}
