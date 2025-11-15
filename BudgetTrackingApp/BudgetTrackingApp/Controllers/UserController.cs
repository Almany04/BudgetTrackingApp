using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserLogic _userLogic;
        private readonly SignInManager<AppUser> _signInManager; 
        private readonly UserManager<AppUser> _userManager;
        public UserController(IUserLogic userLogic, SignInManager<AppUser> signInManager,
                            UserManager<AppUser> userManager)
        {
            _userLogic = userLogic;
            _signInManager = signInManager;
            _userManager = userManager;
        }
       
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> UserRegisterAsync([FromBody]UserRegisterDto userRegisterDto)
        {
            try
            {
                var result =await _userLogic.RegisterUserAsync(userRegisterDto);
                if (result.Succeeded)
                {
                    return StatusCode(201);
                }
                return BadRequest(result.Errors.Select(r=>r.Description));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> UserLoginAsync([FromBody]UserLoginDto userLoginDto)
        {
            try
            {
                // A UserLogic leellenőrzi a jelszót és visszaadja a DTO-t
                var responseDto = await _userLogic.LoginUserAsync(userLoginDto);

                // Most TÉNYLEGESEN bejelentkeztetjük a felhasználót a szerveren,
                // ami beállítja az authentikációs SÜTIT.
                var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
                await _signInManager.SignInAsync(user, isPersistent: false); // <-- EZ A KULCSOR!

                // Visszaadjuk a DTO-t a kliensnek, hogy a localStorage-ba is bekerüljön
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("logout")]
        [Authorize] 
        public async Task<IActionResult> UserLogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync(); 
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
