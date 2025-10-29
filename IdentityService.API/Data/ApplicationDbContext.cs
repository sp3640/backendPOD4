using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <--- This now works!
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.IdentityService.Models;

namespace OnlineAuctionSystem.IdentityService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}