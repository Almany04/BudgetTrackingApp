using BudgetTrackingApp.Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

                // SECURITY FIX: Basic Base64 Validation
                if (string.IsNullOrEmpty(request.ImageBase64) || request.ImageBase64.Length > 10_000_000) // 10MB Limit
                {
                    return BadRequest("Invalid image data.");
                }

                // Simple check to see if it looks like an image (starts with /9j/ for JPG, iVBOR for PNG, etc.)
                // This is not fool-proof but filters obvious garbage.
                if (!Regex.IsMatch(request.ImageBase64, @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"))
                {
                    return BadRequest("Invalid Base64 format.");
                }

                var result = await _aiLogic.ScanReceiptAsync(request.ImageBase64, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receipt Scan Failed");
                // SECURITY FIX: Do not return raw exception to client
                return StatusCode(500, "An error occurred while scanning the receipt.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                return Ok(await _aiLogic.GenerateStructuredAdviceAsync(userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Advice Failed");
                return StatusCode(500, "An error occurred while generating advice.");
            }
        }
    }
}