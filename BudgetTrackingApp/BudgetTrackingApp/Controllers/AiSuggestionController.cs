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
    // CHANGED: Use "General" (60 req/min) instead of "Strict" to fix 429 errors
    [EnableRateLimiting("General")]
    public class AiSuggestionController : ControllerBase
    {
        private readonly IAiSuggestionLogic _aiLogic;
        private readonly ILogger<AiSuggestionController> _logger;

        public AiSuggestionController(IAiSuggestionLogic aiLogic, ILogger<AiSuggestionController> logger)
        {
            _aiLogic = aiLogic;
            _logger = logger;
        }

        public class ScanRequest { public string ImageBase64 { get; set; } }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanReceipt([FromBody] ScanRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var result = await _aiLogic.ScanReceiptAsync(request.ImageBase64, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the actual error to the server console
                _logger.LogError(ex, "Receipt Scan Failed");
                // Return the specific message so you can see it in the browser network tab
                return BadRequest($"Scan failed: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Ok(await _aiLogic.GenerateStructuredAdviceAsync(userId!));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}