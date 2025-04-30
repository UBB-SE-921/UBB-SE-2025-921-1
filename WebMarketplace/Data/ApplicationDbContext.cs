using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedClassLibrary.Domain;

namespace WebMarketplace.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SharedClassLibrary.Domain.User> Users { get; set; } = default!;
        public DbSet<SharedClassLibrary.Domain.Address> Addresses { get; set; } = default!;
    }
}
