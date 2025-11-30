using BudgetTrackingApp.Data;
using BudgetTrackingApp.Data.Entities;

using BudgetTrackingApp.Shared.Dtos.Transactions;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeaturesController : ControllerBase
    {
        private readonly BudgetTrackerDbContext _context;

        public FeaturesController(BudgetTrackerDbContext context)
        {
            _context = context;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // --- SHOPPING LIST ---
        [HttpGet("shopping")]
        public async Task<IActionResult> GetShoppingList()
        {
            var items = await _context.ShoppingItems.Where(x => x.AppUserId == UserId).ToListAsync();
            return Ok(items);
        }

        [HttpPost("shopping")]
        public async Task<IActionResult> AddShoppingItem([FromBody] ShoppingItemDto dto)
        {
            var item = new ShoppingItem { Name = dto.Name, AppUserId = UserId, IsBought = false };
            _context.ShoppingItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("shopping/{id}")]
        public async Task<IActionResult> ToggleShoppingItem(Guid id)
        {
            var item = await _context.ShoppingItems.FirstOrDefaultAsync(x => x.Id == id && x.AppUserId == UserId);
            if (item == null) return NotFound();
            item.IsBought = !item.IsBought;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("shopping/{id}")]
        public async Task<IActionResult> DeleteShoppingItem(Guid id)
        {
            var item = await _context.ShoppingItems.FirstOrDefaultAsync(x => x.Id == id && x.AppUserId == UserId);
            if (item != null) { _context.ShoppingItems.Remove(item); await _context.SaveChangesAsync(); }
            return Ok();
        }

        // --- SAVING GOALS ---
        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals()
        {
            var goals = await _context.SavingGoals
                .Include(g => g.Transactions)
                .Where(g => g.AppUserId == UserId)
                .ToListAsync();

            var dtos = goals.Select(g => new SavingGoalDto
            {
                Id = g.Id,
                Name = g.Name,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.Transactions.Sum(t => t.Amount) // Calculate progress dynamically
            });
            return Ok(dtos);
        }

        [HttpPost("goals")]
        public async Task<IActionResult> AddGoal([FromBody] SavingGoalDto dto)
        {
            var goal = new SavingGoal { Name = dto.Name, TargetAmount = dto.TargetAmount, AppUserId = UserId };
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}