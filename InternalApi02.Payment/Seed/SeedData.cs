using InternalApi02.Payment.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalApi02.Payment.Seed
{
    public class SeedData
    {
        public static void Init(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            context.Database.EnsureCreated();

            if (context.Payments.Any()) return;
            context.Payments.AddRange(
                new Models.PaymentItem { TransactionCode = "X1", Value = 49.90m },
                new Models.PaymentItem { TransactionCode = "X2", Value = 199.90m }
            );

            context.SaveChanges();
        }
    }
}
