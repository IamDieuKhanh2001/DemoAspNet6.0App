﻿using DemoAspNetApp.Models;
using DemoAspNetApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoAspNetApp.Controllers
{
    //khanh@gmail.com 123456@Abc

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await accountRepository.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok("Create account success for user: " + signUpModel.Email);
            }

            return Unauthorized("Can not sign up");
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var result = await accountRepository.SignInAsync(signInModel);
            if (result == null)
            {
                return Unauthorized("Can not sign in, email or password maybe incorrect");
            }

            return Ok(result);
        }
    }
}
