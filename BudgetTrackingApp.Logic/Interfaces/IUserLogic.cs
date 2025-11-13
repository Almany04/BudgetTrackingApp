using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Logic.Interfaces
{
    public interface IUserLogic
    {
        Task<IdentityResult> RegisterUserAsync(UserRegisterDto userRegisterDto);
        Task<UserLoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto);
    }
}
