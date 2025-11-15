using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryLogic _categoryLogic;
        public CategoriesController(ICategoryLogic categoryLogic)
        {
            _categoryLogic=categoryLogic;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) {
                throw new Exception("Felhasználó ID nem található a tokenben. ");
            }
            return userId;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserCategories()
        {
            try
            {
                string testUserId = GetUserId();
                var categoriesDto = await _categoryLogic.GetCategoriesByUserIdAsync(testUserId);
                return Ok(categoriesDto);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody]CategoryCreateDto categoryCreateDto)
        {
            try
            {
                string testUserId = GetUserId();
                await _categoryLogic.CreateCategoryAsync(categoryCreateDto, testUserId);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            try
            {
                string userId = GetUserId();
                await _categoryLogic.UpdateCategoryAsync(id, categoryUpdateDto, userId);
                return Ok(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                string userId = GetUserId();
                await _categoryLogic.DeleteCategoryAsync(id, userId);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }
    }
}
