using DemoAspNetApp.Models;
using Microsoft.AspNetCore.Identity;

namespace DemoAspNetApp.Services
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<SignInResponse> SignInAsync(SignInModel model);
    }
}
