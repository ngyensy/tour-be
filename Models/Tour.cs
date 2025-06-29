using System.Collections.Generic;
using System;
using WebApi.Entities;

public class TourModel
{
    public string Id { get; set; } // ID của tour
    public string Name { get; set; } // Tên của tour
    public string Description { get; set; } // Mô tả về tour
    public int Duration { get; set; } // Thời gian tour (số ngày)

    // Giá dành cho người lớn và trẻ em
    public decimal Price { get; set; } // Giá người lớn
    public decimal ChildPrice { get; set; } // Giá trẻ em
    public decimal SingleRoomSurcharge { get; set; }

     // Thêm thuộc tính giảm giá
    public decimal? Discount { get; set; } // Mức giảm giá (số tiền hoặc phần trăm)

    public string DepartureLocation { get; set; } // Nơi khởi hành
    public string Destination { get; set; } // Điểm đến

    public DateTime StartDate { get; set; } // Ngày bắt đầu
    public DateTime EndDate { get; set; } // Ngày kết thúc

    public int AvailableSlots { get; set; } // Số chỗ còn lại
    public bool IsActive { get; set; } // Trạng thái hoạt động
    public string Image { get; set; } // URL hình ảnh

    // Foreign key to Category
    public int CategoryId { get; set; } // Liên kết với ID của Category
    public Category Category { get; set; } // Đối tượng Category

    // Thuộc tính cho lịch trình
    public List<Itinerary> Itineraries { get; set; }

    // Mới thêm: Các ngày khởi hành của tour
    public List<TourSchedule> TourSchedules { get; set; }
}