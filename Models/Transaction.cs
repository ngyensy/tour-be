using System;
using WebApi.Entities;

namespace WebApi.Models
{
    public class TransactionModel
    {
        public string Id { get; set; }
        public string BookingId { get; set; }
        public Booking Booking { get; set; }
        public int? UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
