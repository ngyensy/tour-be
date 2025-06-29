using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApi.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Avatar")]
        public string Avatar { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Column("Address")]
        public string Address { get; set; }

        [Column("Role")]
        public string Role { get; set; }

        [Column("PasswordHash")]
        [JsonIgnore]
        public string PasswordHash { get; set; }

        // Thêm trường Giới tính
        [Column("Gender")]
        public string Gender { get; set; }

        // Thêm trường Ngày sinh
        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("RewardPoints")]
        public int RewardPoints { get; set; }

        [Column("NumberOfToursTaken")]
        public int NumberOfToursTaken { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
