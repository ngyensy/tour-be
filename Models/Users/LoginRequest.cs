using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } // Có thể là Email hoặc Số điện thoại

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
