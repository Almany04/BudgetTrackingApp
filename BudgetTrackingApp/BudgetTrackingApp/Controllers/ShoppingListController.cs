using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [Route("api/transaction/shopping")] // Matches the path used in Client
    [ApiController]
    [Authorize]
    public class ShoppingListController : ControllerBase
    {
        private readonly IShoppingListLogic _logic;

        public ShoppingListController(IShoppingListLogic logic)
        {
            _logic = logic;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet]
        public async Task<ActionResult<List<ShoppingItemDto>>> Get()
        {
            return Ok(await _logic.GetListAsync(UserId));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ShoppingItemDto dto)
        {
            await _logic.AddItemAsync(UserId, dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Toggle(Guid id, [FromBody] ShoppingItemDto dto)
        {
            await _logic.ToggleItemAsync(UserId, id, dto.IsBought);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _logic.DeleteItemAsync(UserId, id);
            return Ok();
        }
    }
}
