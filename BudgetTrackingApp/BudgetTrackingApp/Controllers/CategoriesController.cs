using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [IgnoreAntiforgeryToken] // FIX: Unblocks creation
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
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            try { await _categoryLogic.CreateCategoryAsync(dto, GetUserId()); return StatusCode(201); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto dto)
        {
            try { await _categoryLogic.UpdateCategoryAsync(id, dto, GetUserId()); return Ok(); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try { await _categoryLogic.DeleteCategoryAsync(id, GetUserId()); return NoContent(); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}