using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Budget;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class BudgetController : ControllerBase
    {
        private readonly IBudgetLogic _budgetLogic;
        public BudgetController(IBudgetLogic budgetLogic)
        {
            _budgetLogic = budgetLogic;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                throw new Exception("Felhasználó ID nem található a tokenben. ");
            }
            return userId;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserBudget()
        {
            try
            {
                string testUserId = GetUserId();
                var budgetDto =await _budgetLogic.GetBudgetByUserIdAsync(testUserId);
                if (budgetDto == null)
                {
                    return NotFound("A felhasználóhoz tartozó budget nem található.");
                }

                return Ok(budgetDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateBudgetLimit([FromBody] BudgetUpdateDto budgetUpdateDto)
        {
            try
            {
                string testUserId = GetUserId();
                await _budgetLogic.UpdateBudgetLimitAsync(budgetUpdateDto, testUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
