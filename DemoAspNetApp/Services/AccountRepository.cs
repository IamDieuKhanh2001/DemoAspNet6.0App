﻿using DemoAspNetApp.Data;
using DemoAspNetApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoAspNetApp.Services
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        public BinaryReader JwtRegisteredClaimName { get; private set; }

        public async Task<SignInResponse> SignInAsync(SignInModel model)
        {
            var result = await signInManager.PasswordSignInAsync
                (model.Email, model.Password, false, false);
            if (!result.Succeeded)
            {
                return null; //Can not sign in
            }

            var userLogin = await userManager.FindByEmailAsync(model.Email);
            var userRoles = await userManager.GetRolesAsync(userLogin);
            var roleClaims = userRoles.Select(role => new Claim(ClaimTypes.Role, role));
            
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            authClaims.AddRange(roleClaims);

            var authenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT:Secret"])
                );

            var tokenInfo = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(20),
                claims: authClaims,
                signingCredentials: new SigningCredentials(
                    authenKey,
                    SecurityAlgorithms.HmacSha512Signature
                    )
                );

            var jwtTokenHash = new JwtSecurityTokenHandler().WriteToken(tokenInfo);

            var response = new SignInResponse
            {
                JwtToken = jwtTokenHash,
                User = new UserVM
                {
                    Id = userLogin.Id,
                    Email = userLogin.Email,
                    FirstName = userLogin.FirstName,
                    LastName = userLogin.LastName,
                    UserName = userLogin.UserName,
                    Roles = await GetRolesUser(userLogin.UserName),
                }
            };

            return response;
        }

        public async Task<List<RoleVM>> GetRolesUser(string username)
        {
            var user = await userManager.FindByEmailAsync(username);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);

                var roleVMs = roles.Select(roleName => new RoleVM
                {
                    RoleId = roleManager.Roles.First(r => r.Name == roleName).Id,
                    RoleName = roleName
                }).ToList();

                return roleVMs;
            }

            return new List<RoleVM>();
        }


        public async Task<IdentityResult> SignUpAsync(SignUpModel model)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await userManager.CreateAsync(user, model.Password);//Fw luu vao asp net users

            await userManager.AddToRoleAsync(user, "Member");

            return result; 
        }
    }
}
