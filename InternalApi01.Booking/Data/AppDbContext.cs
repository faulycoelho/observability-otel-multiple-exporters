using InternalApi01.Booking.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi01.Booking.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Models.Booking> Bookings => Set<Models.Booking>();
    }
}
