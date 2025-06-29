using System;
using System.ComponentModel.DataAnnotations;
using WebApi.Models.Users;

namespace WebApi.Models
{
    public class BookingModel
    {
        public string Id { get; set; } // ID của booking

        // Thông tin User nếu có tài khoản
        public int? UserId { get; set; } // ID người dùng (có thể null nếu là guest)
        public CreateRequest User { get; set; }

        // Thông tin Guest nếu không có tài khoản
        public string GuestName { get; set; } // Tên của guest
        public string GuestEmail { get; set; } // Email của guest
        public string GuestPhoneNumber { get; set; } // Số điện thoại của guest
        public string GuestAddress { get; set; }

        public string TourId { get; set; } // ID tour được đặt
        public TourModel Tour { get; set; }

        public DateTimeOffset BookingDate { get; set; } // Ngày đặt

        // Số lượng người lớn và trẻ em
        public int NumberOfAdults { get; set; } // Số người lớn
        public int NumberOfChildren { get; set; } // Số trẻ em
        public int NumberOfPeople => NumberOfAdults + NumberOfChildren; // Tính tổng số người

        public decimal TotalPrice { get; set; } // Tổng tiền (số người * giá tour)
        public decimal TotalSingleRoomSurcharge { get; set; } 
        public string Status { get; set; } = "Chờ xác nhận"; // Trạng thái của booking (Pending, Confirmed, Cancelled)

        public string Notes { get; set; }
        public string PaymentMethod { get; set; }

        public int? TourScheduleId {get; set;}

        public string AppliedCode { get; set; }


    }
}
