using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfilesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var updateUser = user.Adapt<UpdatePersonalInfoResponse>();

            return Ok(updateUser);
        }



        [HttpPut("Update")]
        public async Task<IActionResult> UpdateInfo(UpdatePersonalInfoRequest updatePersonalInfoRequest)
        {
           

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            user.Name = updatePersonalInfoRequest.Name;
            user.Email = updatePersonalInfoRequest.Email;
            user.PhoneNumber = updatePersonalInfoRequest.PhoneNumber;
            user.Street = updatePersonalInfoRequest.Street;
            user.State = updatePersonalInfoRequest.State;
            user.City = updatePersonalInfoRequest.City;
            user.ZipCode = updatePersonalInfoRequest.ZipCode;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}
