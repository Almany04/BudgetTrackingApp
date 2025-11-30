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
    [IgnoreAntiforgeryToken] // Note: Consider removing this if you implement proper CSRF tokens later
    [EnableRateLimiting("General")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryLogic _categoryLogic;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryLogic categoryLogic, ILogger<CategoriesController> logger)
        {
            _categoryLogic = categoryLogic;
            _logger = logger;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

        [HttpGet]
        public async Task<IActionResult> GetUserCategories()
        {
            try { return Ok(await _categoryLogic.GetCategoriesByUserIdAsync(GetUserId())); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            try
            {
                await _categoryLogic.CreateCategoryAsync(dto, GetUserId());
                var categories = await _categoryLogic.GetCategoriesByUserIdAsync(GetUserId());
                var created = categories.FirstOrDefault(c => c.Name == dto.Name);
                return StatusCode(201, created);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return BadRequest(ex.Message); // Safe to return Logic messages like "Duplicate name"
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto dto)
        {
            try { await _categoryLogic.UpdateCategoryAsync(id, dto, GetUserId()); return Ok(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try { await _categoryLogic.DeleteCategoryAsync(id, GetUserId()); return NoContent(); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return BadRequest(ex.Message);
            }
        }
    }
}