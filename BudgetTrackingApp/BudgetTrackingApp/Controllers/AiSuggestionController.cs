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
                return BadRequest("Váratlan hiba történt.");
            }
        }
        [HttpPost("scan-receipt")]
        public async Task<IActionResult> ScanReceipt(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image uploaded.");

            // Max 5MB limit a biztonság kedvéért
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image too large (max 5MB).");

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var result = await _aiLogic.ScanReceiptAsync(bytes, file.ContentType);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}