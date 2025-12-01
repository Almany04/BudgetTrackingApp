using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FeaturesController : ControllerBase
    {
        private readonly ISavingGoalLogic _savingGoalLogic;

        public FeaturesController(ISavingGoalLogic savingGoalLogic)
        {
            _savingGoalLogic = savingGoalLogic;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --- SAVING GOALS ---

        [HttpGet("goals")]
        public async Task<ActionResult<List<SavingGoalDto>>> GetGoals()
        {
            var goals = await _savingGoalLogic.GetGoalsAsync(UserId);
            return Ok(goals);
        }

        [HttpPost("goals")]
        public async Task<IActionResult> CreateGoal([FromBody] SavingGoalDto dto)
        {
            await _savingGoalLogic.CreateGoalAsync(UserId, dto);
            return Ok();
        }

        [HttpPut("goals/{id}")]
        public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] SavingGoalUpdateDto dto)
        {
            var success = await _savingGoalLogic.UpdateGoalAsync(UserId, id, dto);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpDelete("goals/{id}")]
        public async Task<IActionResult> DeleteGoal(Guid id)
        {
            var success = await _savingGoalLogic.DeleteGoalAsync(UserId, id);
            if (!success) return NotFound();

            return Ok();
        }
    }
}