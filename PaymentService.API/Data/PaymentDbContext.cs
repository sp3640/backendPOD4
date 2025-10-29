using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.PaymentService.Models;

namespace OnlineAuctionSystem.PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure fast lookups by AuctionId, as this is how transactions are often queried
            builder.Entity<Transaction>()
                .HasIndex(t => t.AuctionId)
                .IsUnique();
        }
    }
}