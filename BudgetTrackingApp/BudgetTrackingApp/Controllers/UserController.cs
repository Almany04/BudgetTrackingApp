using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [IgnoreAntiforgeryToken] // FIX 1: Allows POST without token issues
        public async Task<IActionResult> UserRegisterAsync([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                var result = await _userLogic.RegisterUserAsync(userRegisterDto);
                if (result.Succeeded) return StatusCode(201);
                return BadRequest(result.Errors.Select(r => r.Description));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken] // FIX 1
        public async Task<IActionResult> UserLoginAsync([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                var responseDto = await _userLogic.LoginUserAsync(userLoginDto);
                var user = await _userManager.FindByEmailAsync(userLoginDto.Email);

                // FIX 2: Persistent cookie for mobile/refresh
                await _signInManager.SignInAsync(user, isPersistent: true);

                return Ok(responseDto);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        // FIX 3: Endpoint to recover session after refresh
        [HttpGet("current")]
        [AllowAnonymous]
        public IActionResult GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new UserLoginResponseDto
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                    Email = User.FindFirstValue(ClaimTypes.Email) ?? ""
                });
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UserLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}