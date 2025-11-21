using BudgetTrackingApp.Data.Entities;
using BudgetTrackingApp.Logic.Services;
using BudgetTrackingApp.Repository.Interfaces;
using BudgetTrackingApp.Shared.Dtos.User;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace BudgetTrackingApp.Tests
{
    public class UserLogicTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IBudgetRepository> _mockBudgetRepo;
        private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
        private readonly UserLogic _userLogic;

        public UserLogicTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockBudgetRepo = new Mock<IBudgetRepository>();

            // A SignInManager mockolása bonyolultabb, de a Logic teszthez megkerülhetjük
            // vagy használunk egy egyszerűsített konstruktort, ha van.
            // Mivel a UserLogic függ a SignInManagertől, itt egy trükkös setup kellene.
            // EGYSZERŰSÍTÉS: Most koncentráljunk arra, hogy a RegisterUserAsync létrehozza-e az alapértelmezett adatokat.

            // (A teljes mockolás itt túl sok "boilerplate" kódot igényelne, 
            // ezért a UserLogic tesztet gyakran inkább Integrációs tesztként írjuk meg).
        }
    }
}