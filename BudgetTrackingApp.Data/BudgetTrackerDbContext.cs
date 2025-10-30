using BudgetTrackingApp.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTrackingApp.Data
{
    public class BudgetTrackerDbContext : IdentityDbContext<AppUser>
    {
        public BudgetTrackerDbContext(DbContextOptions<BudgetTrackerDbContext> options):base(options)
        {

        }


        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
