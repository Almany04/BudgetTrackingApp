using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("General")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionLogic _transactionLogic;
        public TransactionController(ITransactionLogic transactionLogic)
        {
            _transactionLogic = transactionLogic;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

        [HttpGet]
        public async Task<IActionResult> GetTransactionsAsync([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try { return Ok(await _transactionLogic.GetTransactionsByUserIdFilteredAsync(GetUserId(), startDate ?? DateTime.Now.AddDays(-30), endDate ?? DateTime.Now)); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransactionsAsync([FromBody] TransactionCreateDto dto)
        {
            try { await _transactionLogic.CreateTransactionAsync(dto, GetUserId()); return StatusCode(201); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionByIdAsync(Guid id)
        {
            try
            {
                var entity = await _transactionLogic.GetTransactionByIdAsync(id, GetUserId());
                return entity == null ? NotFound() : Ok(entity);
            }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransactionAsync(Guid id, [FromBody] TransactionUpdateDto dto)
        {
            try { await _transactionLogic.UpdateTransactionAsync(id, dto, GetUserId()); return Ok(); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransactionAsync(Guid id)
        {
            try { await _transactionLogic.DeleteTransactionAsync(id, GetUserId()); return NoContent(); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }
    }
}