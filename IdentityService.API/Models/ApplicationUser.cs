using Microsoft.AspNetCore.Identity;

namespace OnlineAuctionSystem.IdentityService.Models
{
    // Extends IdentityUser to add custom properties, like Role, 
    // though using IdentityRole is the best practice for production,
    // this simplifies the simulation.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // This Role property is used for easy JWT Claim inclusion 
        // and initial registration simplicity.
        public string Role { get; set; }
    }
}