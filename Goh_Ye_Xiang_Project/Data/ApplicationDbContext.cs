using CozyPlaceSG.Models;
using Microsoft.EntityFrameworkCore;

namespace Goh_Ye_Xiang_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Booking>? Bookings { get; set; }
    }
}
