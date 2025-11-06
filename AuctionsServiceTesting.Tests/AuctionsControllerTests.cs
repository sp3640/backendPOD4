using NUnit.Framework;

using Microsoft.AspNetCore.Mvc;

using OnlineAuctionSystem.AuctionService.Controllers;

using OnlineAuctionSystem.AuctionService.Data;

using OnlineAuctionSystem.AuctionService.DTOs;

using OnlineAuctionSystem.AuctionService.Models;

using AuctionServiceTesting.Tests.Helpers;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;

using System.Security.Claims;

using System;

using System.Threading.Tasks;

namespace AuctionsServiceTesting.Tests

{

    [TestFixture]

    public class AuctionsControllerTests

    {

        private AuctionDbContext _context;

        private AuctionsController _controller;

        [SetUp]

        public void Setup()

        {

            _context = DbContextMockHelper.GetInMemoryDbContext();

            _controller = new AuctionsController(_context);

        }

        [TearDown]

        public void Cleanup()

        {

            _context.Dispose();

        }

        [Test]

        public async Task GetAuctions_ShouldReturnEmptyList_WhenNoAuctionsExist()

        {

            var result = await _controller.GetAuctions();

            Assert.IsNotNull(result.Value);

            Assert.AreEqual(0, result.Value.Count());

        }

        [Test]

        public async Task GetAuction_ShouldReturnNotFound_WhenAuctionDoesNotExist()

        {

            var result = await _controller.GetAuction(Guid.NewGuid());

            Assert.IsInstanceOf<NotFoundResult>(result.Result);

        }

        [Test]

        public async Task CreateAuction_ShouldAddAuction_WhenValidDataProvided()

        {

            var dto = new AuctionCreateDto

            {

                ProductName = "Laptop",

                Description = "Gaming Laptop",

                StartPrice = 500,

                DurationMinutes = 60,

                ImageUrl = "http://image.com/laptop.jpg"

            };

            _controller.ControllerContext = new ControllerContext

            {

                HttpContext = new DefaultHttpContext()

            };

            _controller.HttpContext.User = new ClaimsPrincipal(

                new ClaimsIdentity(new[]

                {

                    new Claim(ClaimTypes.Name, "sellerUser"),

                    new Claim(ClaimTypes.Role, "Seller")

                }, "mock"));

            var result = await _controller.CreateAuction(dto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result);

            Assert.AreEqual(1, await _context.Auctions.CountAsync());

        }

    }

}