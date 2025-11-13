using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Shared.Dtos.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryLogic _categoryLogic;
        public CategoriesController(ICategoryLogic categoryLogic)
        {
            _categoryLogic=categoryLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCategories()
        {
            try
            {
                string testUserId = "TESZT_USER_ID";
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
                string testUserId = "TESZT_USER_ID";
                await _categoryLogic.CreateCategoryAsync(categoryCreateDto, testUserId);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
