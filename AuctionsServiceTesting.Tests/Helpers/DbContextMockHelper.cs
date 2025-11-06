using Microsoft.EntityFrameworkCore;

using OnlineAuctionSystem.AuctionService.Data;

using System;

namespace AuctionServiceTesting.Tests.Helpers

{

    public static class DbContextMockHelper

    {

        public static AuctionDbContext GetInMemoryDbContext()

        {

            var options = new DbContextOptionsBuilder<AuctionDbContext>()

                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test

                .Options;

            return new AuctionDbContext(options);

        }

    }

}