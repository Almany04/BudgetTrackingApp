using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserLogic _userLogic;
        public UserController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        [HttpPost("register")]
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
        public async Task<IActionResult> UserLoginAsync([FromBody]UserLoginDto userLoginDto)
        {
            try
            {
                var responseDto=await _userLogic.LoginUserAsync(userLoginDto);
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
