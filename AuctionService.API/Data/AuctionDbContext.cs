using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.AuctionService.Models;

namespace OnlineAuctionSystem.AuctionService.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

        public DbSet<Auction> Auctions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Auction>(entity =>
            {
                // FIX WARNING 1: Explicitly set SQL type for StartPrice
                // decimal(18, 2) is a standard choice for currency
                entity.Property(a => a.StartPrice)
                      .HasPrecision(18, 2);

                // FIX WARNING 2: Explicitly set SQL type for HighestBid
                entity.Property(a => a.HighestBid)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0.00m); // Kept your default value setting
            });
        }
    }
}