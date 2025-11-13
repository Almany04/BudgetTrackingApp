using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Interfaces;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Services
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;

        public UserLogic(IUserRepository userRepository, ICategoryRepository categoryRepository, IBudgetRepository budgetRepository)
        {
            _userRepository=userRepository;
            _categoryRepository=categoryRepository;
            _budgetRepository=budgetRepository;

        }
        public async Task<UserLoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);
            if (user == null)
            {
                throw new Exception("Hibás email cím vagy jelszó!");
            }
            
            var isPasswordCorrect= await _userRepository.CheckPasswordAsync(user,userLoginDto.Password);
            if (isPasswordCorrect==false)
            {
                throw new Exception("Hibás email cím vagy jelszó!");
            }
            return new UserLoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email
            };
        }

        public async Task<IdentityResult> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {

            var newUser = new AppUser
            {
                UserName=userRegisterDto.Email,
                Email=userRegisterDto.Email
            };

            var result= await _userRepository.CreateUserAsync(newUser, userRegisterDto.Password);

            if (result.Succeeded)
            {
                var newBudget = new Budget
                {
                    AppUserId = newUser.Id,
                    LimitAmount = 1000000
                };
                await _budgetRepository.AddBudgetAsync(newBudget);

                var defaultCategory = new Category
                {
                    Name = "Általános",
                    AppUserId = newUser.Id
                };
                await _categoryRepository.AddCategoryAsync(defaultCategory);
            }
            return result;


        }
    }
}
