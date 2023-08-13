using DemoAspNetApp.Data;
using DemoAspNetApp.Helpers;
using DemoAspNetApp.Models;
using DemoAspNetApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DemoAspNetApp.Controllers
{
    //khanh@gmail.com 123456@Abc

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository accountRepository;
        private readonly IConfiguration configuration;
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(
            IAccountRepository accountRepository,
            IConfiguration configuration,
            DatabaseContext databaseContext,
            UserManager<ApplicationUser> userManager
            )
        {
            this.accountRepository = accountRepository;
            this.configuration = configuration;
            this._context = databaseContext;
            this.userManager = userManager;
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

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel tokenModel)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(configuration["JWT:Secret"]);
            var tokenValidateParam = new TokenValidationParameters
            {
                //Tự cấp token
                ValidateIssuer = true,
                ValidateAudience = true,

                //Ký vào token
                ValidAudience = configuration["JWT:ValidAudience"], //Map from appsettings.json
                ValidIssuer = configuration["JWT:ValidIssuer"],//Map from appsettings.json
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes), //Map from appsettings.json

                ValidateLifetime = false, //Không kiểm tra token hết hạn
            };
            try
            {
                //Check 1: Access token valid format

                var tokenInVerification = jwtTokenHandler
                    .ValidateToken(tokenModel.JwtToken, tokenValidateParam, out var validatedToken);

                //Check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals
                        (SecurityAlgorithms.HmacSha512Signature,
                        StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Invalid token"
                            });
                    }
                }

                //Check 3: Check access tken expire
                var utcExpireDate = long.Parse(
                    tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value
                );
                var expireDate = Utils.ConvertUnixTimeToDateTime(utcExpireDate);

                if (expireDate > DateTime.UtcNow)
                {
                    return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Access token has not yet expired"
                            });
                }

                //Check 4: refresh token exist in DB
                var storedToken = _context.AccountTokens
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.RefreshToken == tokenModel.RefreshToken);
                if (storedToken == null)
                {
                    return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Refresh token does not exist"
                            });
                }

                //Check 5: refresh token is used/revoked or not
                if (storedToken.IsUsed)
                {
                    return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Refresh token has been used"
                            });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Refresh token has been revoked"
                            });
                }

                //check 6: Access token id == JwtId in account token
                var jti = tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if(storedToken.JwtId != jti)
                {
                    return Ok(
                            new ApiResponse
                            {
                                Success = false,
                                Message = "Token does not match"
                            });
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                _context.SaveChanges();

                //Create new token
                var token = await accountRepository.GenerateToken(storedToken.User);

                return Ok(
                            new ApiResponse
                            {
                                Success = true,
                                Message = "Renew token success",
                                Data = token
                            });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new ApiResponse
                    {
                        Success = false,
                        Message = "Something went wrong"
                    });
            }
        }
    }
}
