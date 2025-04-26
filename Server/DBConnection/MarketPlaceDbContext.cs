using Microsoft.EntityFrameworkCore;
using SharedClassLibrary.Domain;

namespace Server.DBConnection
{
    public class MarketPlaceDbContext: DbContext
    {
        public MarketPlaceDbContext(DbContextOptions<MarketPlaceDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
