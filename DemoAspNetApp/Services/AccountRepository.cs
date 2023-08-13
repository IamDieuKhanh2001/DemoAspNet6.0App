using DemoAspNetApp.Data;
using DemoAspNetApp.Helpers;
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
        private readonly DatabaseContext _context;

        public AccountRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            DatabaseContext databaseContext
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this._context = databaseContext;
        }

        public BinaryReader JwtRegisteredClaimName { get; private set; }

        public async Task<TokenModel> GenerateToken(ApplicationUser user)
        {
            try
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var roleClaims = userRoles.Select(role => new Claim(ClaimTypes.Role, role)); //Convert Roles to claims

                #region Create JWT 

                #region Add Claims
                string jwtId = Guid.NewGuid().ToString();
                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim("UserID", user.Id)
            };
                authClaims.AddRange(roleClaims);
                #endregion

                var authenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JWT:Secret"])
                    );

                var tokenInfo = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddMinutes(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(
                        authenKey,
                        SecurityAlgorithms.HmacSha512Signature
                        )
                    );

                #endregion
                var jwtTokenHash = new JwtSecurityTokenHandler().WriteToken(tokenInfo);
                string refreshTokenHash = RefreshTokenGenerator.Generate();

                //Stored RefreshToken and jwt in db
                var accountTokenEntity = new AccountToken
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    JwtId = jwtId,
                    RefreshToken = refreshTokenHash,
                    IsUsed = false,
                    IsRevoked = false,
                    IssueAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddSeconds(2),
                };

                await _context.AddAsync(accountTokenEntity);
                await _context.SaveChangesAsync();

                return new TokenModel
                {
                    JwtToken = jwtTokenHash,
                    RefreshToken = refreshTokenHash
                };
            }
            catch
            {
                return null;
            }
        }
        public async Task<SignInResponse> SignInAsync(SignInModel model)
        {
            #region authentication check
            var result = await signInManager.PasswordSignInAsync
                (model.Email, model.Password, false, false);
            if (!result.Succeeded)
            {
                return null; //Can not sign in
            }
            #endregion

            var userLogin = await userManager.FindByEmailAsync(model.Email);
            var token = await GenerateToken(userLogin);
            if (token == null)
            {
                return null; //Can not sign in
            }
            //Response
            var response = new SignInResponse
            {
                JwtToken = token.JwtToken,
                RefreshToken = token.RefreshToken,
                UserInfo = new UserVM
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

            await userManager.AddToRoleAsync(user, "Member");//Fw luu vao asp net users roles

            return result; 
        }

    }
}
