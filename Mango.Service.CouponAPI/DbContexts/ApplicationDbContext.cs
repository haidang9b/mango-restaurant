using Mango.Service.CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.CouponAPI.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CounponId = 1,
                CounponCode = "10OFF",
                DiscountAmount = 10,
            });

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CounponId = 2,
                CounponCode = "20OFF",
                DiscountAmount = 20,
            });

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CounponId = 3,
                CounponCode = "30OFF",
                DiscountAmount = 30,
            });

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CounponId = 4,
                CounponCode = "40OFF",
                DiscountAmount = 40,
            });
        }
    }
}
