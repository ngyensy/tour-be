using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        [Column("Id")]
        public string Id { get; set; }

        [Column("BookingId")]
        public string BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } // Liên kết đến Booking

        [Column("UserId")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Liên kết đến User

        [Column("Amount")]
        public decimal Amount { get; set; } // Tổng số tiền giao dịch

        [Column("PaymentMethod")]
        public string PaymentMethod { get; set; } // "Credit Card", "Bank Transfer", etc.

        [Column("Status")]
        public string Status { get; set; } // "Paid", "Pending", "Failed", etc.

        [Column("TransactionDate")]
        public DateTime TransactionDate { get; set; }
    }
}
