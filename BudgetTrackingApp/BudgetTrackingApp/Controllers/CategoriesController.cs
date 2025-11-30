using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryLogic _categoryLogic;
        public CategoriesController(ICategoryLogic categoryLogic)
        {
            _categoryLogic = categoryLogic;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

        [HttpGet]
        public async Task<IActionResult> GetUserCategories()
        {
            try { return Ok(await _categoryLogic.GetCategoriesByUserIdAsync(GetUserId())); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            try
            {
                await _categoryLogic.CreateCategoryAsync(dto, GetUserId());

                // FIX: Return the created object (or at least the list) so client doesn't have to guess
                // In a perfect world, CreateCategoryAsync returns the Guid. 
                // For now, let's fetch the created item to return it.
                var categories = await _categoryLogic.GetCategoriesByUserIdAsync(GetUserId());
                var created = categories.FirstOrDefault(c => c.Name == dto.Name);

                return StatusCode(201, created); // Return the DTO
            }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto dto)
        {
            try { await _categoryLogic.UpdateCategoryAsync(id, dto, GetUserId()); return Ok(); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try { await _categoryLogic.DeleteCategoryAsync(id, GetUserId()); return NoContent(); }
            catch (Exception ex) { return BadRequest("Váratlan hiba történt."); }
        }
    }
}