using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApi.Entities;

namespace WebApi.Models.Users
{
    public class CreateRequest
    {
        public int Id { get; set; }

        public string Avatar {get; set;}
        public string Name { get; set; }
        
        public string Role { get; set; } = "User"; 

        public ICollection<Booking> Bookings { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public int RewardPoints { get; set; } = 0;
    }
}