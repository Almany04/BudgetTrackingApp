using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [IgnoreAntiforgeryToken] // FIX: Unblocks updates
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetLogic _budgetLogic;
        public BudgetController(IBudgetLogic budgetLogic) { _budgetLogic = budgetLogic; }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

        [HttpGet]
        public async Task<IActionResult> GetUserBudget()
        {
            try
            {
                var dto = await _budgetLogic.GetBudgetByUserIdAsync(GetUserId());
                return dto == null ? NotFound() : Ok(dto);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBudgetLimit([FromBody] BudgetUpdateDto dto)
        {
            try { await _budgetLogic.UpdateBudgetLimitAsync(dto, GetUserId()); return Ok(); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}