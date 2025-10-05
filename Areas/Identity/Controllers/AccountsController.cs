using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.API.Areas.Identity.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration config;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<UserOTP> _userOTP;
        public AccountsController(UserManager<ApplicationUser> userManager,IConfiguration config ,IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IRepository<UserOTP> userOTP)
        {
            _userManager = userManager;
            this.config = config;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userOTP = userOTP;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
         

            ApplicationUser applicationUser = new()
            {
                Name = registerDTO.Name,
                Email = registerDTO.Email,
                City = registerDTO.City,
                Street = registerDTO.Street,
                State = registerDTO.State,
                ZipCode = registerDTO.ZipCode,
                UserName = registerDTO.UserName
            };

            //ApplicationUser applicationUser = registerVM.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(applicationUser, registerDTO.Password);

            if (!result.Succeeded)
            {
                //foreach (var item in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, item.Description);
                //}

                return BadRequest(result.Errors);
            }

            // Add user to customer role
            await _userManager.AddToRoleAsync(applicationUser, SD.CustomerRole);

            // Send confirmation msg
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = applicationUser.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(applicationUser.Email, $"Confirm Your Email!", $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            return Ok(new 
            {
                msg = "Create User successfully, Confirm Your Email!"
            });
        }

        

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
          

            var user = await _userManager.FindByEmailAsync(loginDTO.EmailOrUserName) ?? await _userManager.FindByNameAsync(loginDTO.EmailOrUserName);

            if (user is null)
            {
                return NotFound(new NotificationDTO
                {
                    Msg = "Invalid User name Or password",
                    TraceID = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow   
                    

                });
            }

            //_userManager.CheckPasswordAsync();
            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberMe, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new 
                    {
                        msg = "Too many attempts"
                    });

                return NotFound(new 
                {
                    msg = "Invalid User name Or password"
                });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new
                {
                    msg = "Confirm Your Email First!"
                });
            }

            if (!user.LockoutEnabled)
            {
                return BadRequest(new
                {
                    msg = $"You have a block till {user.LockoutEnd}"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var Claims = new List<Claim>() 
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Email,user.Email)

            };

            foreach (var item in roles)
            {

                Claims.Add(new Claim(ClaimTypes.Role, item));
            }

            SymmetricSecurityKey Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:key"]??" "));
            SigningCredentials signingCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config["JWT:issuer"],
                audience: config["JWT:audience"],
                claims: Claims,
                expires:DateTime.UtcNow.AddMinutes(50),
                signingCredentials: signingCredentials
                );


            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest(new
                {
                    msg = "Link Expired!, Resend Email Confirmation"
                });

            return Ok(new
            {
                msg = "Confirm Email successfully"
            });
        }

        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationDTO resendEmailConfirmationDTO)
        {
            

            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationDTO.EmailOrUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationDTO.EmailOrUserName);

            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User name Or Email"
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new
                {
                    msg = "Already Confirmed!"
                });
            }

            // Send confirmation msg
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, $"Confirm Your Email!", $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            return Ok(new
            {
                msg = "Send Email successfully, Confirm Your Email!"
            });
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO forgetPasswordDTO)
        {
           

            var user = await _userManager.FindByEmailAsync(forgetPasswordDTO.EmailOrUserName) ?? await _userManager.FindByNameAsync(forgetPasswordDTO.EmailOrUserName);

            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User name Or Email"
                });
            }

            // Send confirmation msg
            var OTPNumber = new Random().Next(1000, 9999);
            var link = Url.Action("ResetPassword", "Account", new { area = "Identity", userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, $"Reset Password!", $"<h1>Reset Password Using {OTPNumber}. Don't share it!</h1>");

            await _userOTP.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTPNumber = OTPNumber.ToString(),
                ValidTo = DateTime.UtcNow.AddDays(1)
            });
            await _userOTP.CommitAsync();

            return Ok(new
            {
                msg = "Send OTP Number to Your Email successfully",
                userId = user.Id
            });

        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
           

            var user = await _userManager.FindByIdAsync(resetPasswordDTO.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User name Or Email"
                });
            }

            var userOTP = (await _userOTP.GetAsync(e => e.ApplicationUserId == resetPasswordDTO.ApplicationUserId)).OrderBy(e => e.Id).LastOrDefault();

            if (userOTP is null)
                return NotFound();

            if (userOTP.OTPNumber != resetPasswordDTO.OTPNumber)
            {
                return BadRequest(new
                {
                    msg = "Invalid OTP"
                });
            }

            if (DateTime.UtcNow > userOTP.ValidTo)
            {
                return BadRequest(new
                {
                   msg = "Expired OTP"
                });
            }

            return Ok(new 
            {
                msg = "Success OTP"
            });
        }

        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordDTO newPasswordDTO)
        {
           

            var user = await _userManager.FindByIdAsync(newPasswordDTO.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User name Or Email"
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordDTO.Password);

            return Ok(new
            {
                msg = "Change Password Successfully!"
            });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }
    }
}
