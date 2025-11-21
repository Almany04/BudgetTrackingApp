using BudgetTrackingApp.Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("Strict")]
    public class AiSuggestionController : ControllerBase
    {
        private readonly IAiSuggestionLogic _aiLogic;

        public AiSuggestionController(IAiSuggestionLogic aiLogic)
        {
            _aiLogic = aiLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var suggestions = await _aiLogic.GenerateSuggestionsAsync(userId);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}