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
            try
            {
                // FIX: Ensure the date range includes the full 'End Date' (up to 23:59:59)
                // When the UI sends "2025-11-30", it binds as "2025-11-30 00:00:00".
                // Transactions created "Today" (e.g. 14:00) would be excluded without this adjustment.
                var finalStart = (startDate ?? DateTime.Now.AddDays(-30)).Date;
                var finalEnd = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

                return Ok(await _transactionLogic.GetTransactionsByUserIdFilteredAsync(GetUserId(), finalStart, finalEnd));
            }
            catch (Exception ex)
            {
                // Ideally log ex here
                return BadRequest("Váratlan hiba történt.");
            }
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

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkTransactionsAsync([FromBody] BulkTransactionCreateDto dto)
        {
            try
            {
                await _transactionLogic.CreateBulkTransactionsAsync(dto, GetUserId());
                return StatusCode(201);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}