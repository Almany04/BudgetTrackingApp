using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Budget;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetLogic _budgetLogic;
        public BudgetController(IBudgetLogic budgetLogic)
        {
            _budgetLogic = budgetLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBudget()
        {
            try
            {
                string testUserId = "TESZT_USER_ID";
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
                string testUserId = "TESZT_USER_ID";
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
