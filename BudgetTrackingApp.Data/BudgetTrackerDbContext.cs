using BudgetTrackingApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data
{
    public class BudgetTrackerDbContext : DbContext
    {
        DbSet<User> Users;
        DbSet<Transactions> Transactions;
        DbSet<Category> Categories;
        DbSet<Budget> Budgets;

        public BudgetTrackerDbContext(DbContextOptions<BudgetTrackerDbContext> options):base(options)
        {

        }
    }
}
