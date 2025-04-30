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

            context.SaveChanges();
        }
    }
}
