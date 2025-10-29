using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.ReviewService.Models;

namespace OnlineAuctionSystem.ReviewService.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensures a user can only review a specific auction/transaction once
            builder.Entity<Review>()
                .HasIndex(r => new { r.AuctionId, r.ReviewerUsername })
                .IsUnique();
        }
    }
}