using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Shared.Dtos.Category;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionLogic _transactionLogic;
        public TransactionController(ITransactionLogic transactionLogic)
        {
            _transactionLogic = transactionLogic;
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
        public async Task<IActionResult> GetTransactionsAsync([FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {

            try
            {
                string testUserId = GetUserId();
                DateTime end = endDate ?? DateTime.Now;
                DateTime start = startDate ?? end.AddDays(-30);


                var transasctionDto = await _transactionLogic.GetTransactionsByUserIdFilteredAsync(testUserId, start, end);
                return Ok(transasctionDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateTransactionsAsync([FromBody] TransactionCreateDto transactionCreateDto)
        {
            try
            {
                string testUserId = GetUserId();
                await _transactionLogic.CreateTransactionAsync(transactionCreateDto, testUserId);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionByIdAsync(Guid id)
        {
            try
            {
                string userId = GetUserId();

                var entity = await _transactionLogic.GetTransactionByIdAsync(id, userId);
                if (entity == null)
                {
                    return NotFound("A tranzakció nem található.");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransactionAsync(Guid id, [FromBody] TransactionUpdateDto transactionUpdateDto)
        {
            try
            {
                string userId = GetUserId();
                await _transactionLogic.UpdateTransactionAsync(id, transactionUpdateDto, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransactionAsync(Guid id)
        {
            try
            {
                string userId = GetUserId();
                await _transactionLogic.DeleteTransactionAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}