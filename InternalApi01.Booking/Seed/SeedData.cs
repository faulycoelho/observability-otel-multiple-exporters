using InternalApi01.Booking.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalApi01.Booking.Seed
{
    public class SeedData
    {
        public static void Init(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            context.Database.EnsureCreated();

            if (context.Bookings.Any()) return;
            context.Bookings.AddRange(
                new Models.Booking { Code = "A1", Price = 49.90m },
                new Models.Booking { Code = "B2", Price = 199.90m }
            );

            context.SaveChanges();
        }
    }
}
