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

            builder.Entity<AppUser>(entity => {
                entity.HasOne(u=> u.Budget)
                    .WithOne(b=>b.AppUser)
                    .HasForeignKey<Budget>(b=>b.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Transactions)
                    .WithOne(t => t.AppUser)
                    .HasForeignKey(t => t.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u=> u.Categories)
                    .WithOne(c=>c.AppUser)
                    .HasForeignKey(c=>c.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Category>(entity => {

                
                entity.HasMany(c => c.Transactions)
                    .WithOne(t => t.Category)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 2. NEW Rule: Hierarchy (Parent/Child Categories)
                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
