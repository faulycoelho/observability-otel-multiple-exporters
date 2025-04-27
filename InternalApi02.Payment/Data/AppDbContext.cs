using InternalApi02.Payment.Models;
using Microsoft.EntityFrameworkCore; 

namespace InternalApi02.Payment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PaymentItem> Payments => Set<PaymentItem>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentItem>()
                .Property(p => p.Value)
                .HasPrecision(18, 2);
        }
    }
}
