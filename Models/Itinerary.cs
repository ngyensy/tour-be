using WebApi.Entities;

namespace WebApi.Models
{
    public class Itinerary
    {
        public int Id { get; set; }
        public string TourId { get; set; }
        public string DayNumber { get; set; }  // Ngày thứ mấy
        public string Description { get; set; }  // Miêu tả lịch trình trong ngày
        // Khóa ngoại tham chiếu đến tour
        public virtual Tour Tour { get; set; }
    }
}
