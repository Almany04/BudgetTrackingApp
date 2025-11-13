using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Shared.Dtos.Category;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionLogic _transactionLogic;
        public TransactionController(ITransactionLogic transactionLogic)
        {
            _transactionLogic = transactionLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionsAsync()
        {
            try
            {
                string testUserId = "TESZT_USER_ID";
                DateTime start= DateTime.Now;
                DateTime end= DateTime.Now;

                var transasctionDto = await _transactionLogic.GetTransactionsByUserIdFilteredAsync(testUserId,start, end);
                return Ok(transasctionDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateTransactionsAsync([FromBody]TransactionCreateDto transactionCreateDto)
        {
            try
            {
                string testUserId = "TESZT_USER_ID";
                await _transactionLogic.CreateTransactionAsync(transactionCreateDto, testUserId);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
