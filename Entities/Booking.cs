using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [Column("Id")]
        public string Id { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Foreign key reference to User entity

        [Column("TourId")]
        public string TourId { get; set; }

        [ForeignKey("TourId")]
        public Tour Tour { get; set; } // Foreign key reference to Tour entity

        [Column("BookingDate")]
        public DateTimeOffset BookingDate { get; set; }

        // Số lượng người lớn và trẻ em
        [Column("NumberOfAdults")]
        public int NumberOfAdults { get; set; }

        [Column("NumberOfChildren")]
        public int NumberOfChildren { get; set; }

        [Column("NumberOfPeople")]
        public int NumberOfPeople => NumberOfAdults + NumberOfChildren; // Tính tổng số người

        // Tổng tiền, tính dựa trên giá của người lớn và trẻ em
        [Column("TotalPrice")]
        public decimal TotalPrice { get; set; }

        [Column("TotalSingleRoomSurcharge")]
        public decimal TotalSingleRoomSurcharge { get; set; }  // Tổng giá phụ thu phòng đơn

        [Column("Status")]
        public string Status { get; set; } // e.g., "Confirmed", "Pending", "Cancelled"

        // Optional fields for guest bookings
        [Column("GuestName")]
        public string GuestName { get; set; }

        [Column("GuestEmail")]
        public string GuestEmail { get; set; }

        [Column("GuestPhoneNumber")]
        public string GuestPhoneNumber { get; set; }

        [Column("GuestAddress")]
        public string GuestAddress { get; set; }

        // Special Requests
        [Column("Notes")]
        public string Notes { get; set; }

        // Payment Information
        [Column("PaymentMethod")]
        public string PaymentMethod { get; set; } // "Credit Card", "Bank Transfer", etc.

        [Column("TourScheduleId")]
        public int? TourScheduleId {get; set;}

        [Column("AppliedCode")]
        public string AppliedCode { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
