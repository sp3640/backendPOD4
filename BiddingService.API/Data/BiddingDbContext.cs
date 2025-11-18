using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.BiddingService.Models;

namespace OnlineAuctionSystem.BiddingService.Data
{
    public class BiddingDbContext : DbContext
    {
        public BiddingDbContext(DbContextOptions<BiddingDbContext> options) : base(options) { }

        // Only track bids in this database
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensures fast lookups on AuctionId and Timestamp
            builder.Entity<Bid>()
                .HasIndex(b => new { b.AuctionId, b.Timestamp });

            // Configure decimal precision for Bid.Amount
            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);
        }
    }
}
