using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using WebApi.Helpers;
using WebApi.Entities;
using WebApi.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public interface IBookingService
    {
        IEnumerable<BookingModel> GetAllBookings();
        BookingModel GetById(string id);
        void Create(BookingModel booking);
        void Update(string id, BookingModel booking);
        void Delete(string id);
        int GetBookingCount();
        decimal GetTotalRevenue();

        List<MonthlyRevenueModel> GetMonthlyRevenue();
        List<PopularCategoryModel> GetPopularCategories();
        List<PopularTourModel> GetPopularTours();
        
    }

    public class BookingService : IBookingService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;

        public BookingService(DataContext context, IMapper mapper, ITransactionService transactionService)
        {
            _context = context;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        public IEnumerable<BookingModel> GetAllBookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.Tour) // Bao gồm thông tin Tour
                .ToList();

            return _mapper.Map<IEnumerable<BookingModel>>(bookings);
        }

        public BookingModel GetById(string id)
        {
            var booking = _context.Bookings
                .Include(b => b.Tour) // Include để load thông tin Tour
                .FirstOrDefault(b => b.Id == id); // Lấy booking theo id

            return _mapper.Map<BookingModel>(booking);
        }

        public void Create(BookingModel bookingModel)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.Id == bookingModel.TourId)
                       ?? throw new ArgumentException("Tour không tồn tại!");

            // Tính tổng số người đặt
            int totalPeople = bookingModel.NumberOfAdults + bookingModel.NumberOfChildren;

            // Kiểm tra số chỗ còn lại
            if (tour.AvailableSlots < totalPeople)
            {
                throw new InvalidOperationException("Số chỗ còn lại không đủ!");
            }

            // Xử lý mã giảm giá nếu có
            if (!string.IsNullOrEmpty(bookingModel.AppliedCode))
            {
                var discountCode = _context.DiscountCodes.FirstOrDefault(dc => dc.Code == bookingModel.AppliedCode) ?? throw new ArgumentException("Mã giảm giá không tồn tại!");
                if (discountCode.IsUsed)
                {
                    throw new InvalidOperationException("Mã giảm giá đã được sử dụng!");
                }

                // Đánh dấu mã giảm giá là đã sử dụng
                discountCode.IsUsed = true;
                _context.DiscountCodes.Update(discountCode);
            }

            // Trừ số chỗ còn lại khỏi tour
            tour.AvailableSlots -= totalPeople;

            if (tour.AvailableSlots == 0)
            {
                tour.IsActive = false;
            }

            // Chuyển đổi BookingModel thành Booking entity
            var bookingEntity = _mapper.Map<Booking>(bookingModel);

            // Tạo BookingId ngẫu nhiên
            bookingEntity.Id = IdGenerator.GenerateBookingId(10);

            // Chỉnh sửa thời gian đặt chỗ, loại bỏ mili giây
            bookingEntity.BookingDate = bookingModel.BookingDate.AddMilliseconds(-bookingModel.BookingDate.Millisecond);

            // Thêm booking vào cơ sở dữ liệu
            _context.Bookings.Add(bookingEntity);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Sau khi tạo booking thành công, tự động tạo giao dịch cho booking này
            var transactionModel = new TransactionModel
            {
                BookingId = bookingEntity.Id, // Gán BookingId cho giao dịch
                UserId = bookingEntity?.UserId,
                PaymentMethod = bookingModel.PaymentMethod,
                Amount = bookingModel.TotalPrice, // Sử dụng tổng giá trị thanh toán của booking
                Status = "Đang xử lí", // Trạng thái giao dịch có thể là "Completed"
                TransactionDate = DateTime.Now // Ngày giờ giao dịch

            };

            // Tạo giao dịch cho booking
            _transactionService.CreateTransaction(transactionModel);
        }

        public void Update(string id, BookingModel bookingModel)
        {
            var existingBooking = _context.Bookings.Include(b => b.Tour).FirstOrDefault(b => b.Id == id);
            if (existingBooking == null) return;

            var tour = existingBooking.Tour;
            if (tour == null)
            {
                throw new ArgumentException("Tour không tồn tại!");
            }

            // Tính tổng số người trước đó
            int previousTotalPeople = existingBooking.NumberOfAdults + existingBooking.NumberOfChildren;

            // Tính tổng số người mới
            int newTotalPeople = bookingModel.NumberOfAdults + bookingModel.NumberOfChildren;

            // Cập nhật lại số chỗ còn lại của tour
            tour.AvailableSlots += previousTotalPeople; // Hoàn lại số chỗ từ booking cũ
            if (tour.AvailableSlots < newTotalPeople)
            {
                throw new InvalidOperationException("Số chỗ còn lại không đủ để cập nhật booking!");
            }
            tour.AvailableSlots -= newTotalPeople; // Trừ số chỗ từ booking mới

            // Cập nhật thông tin booking
            existingBooking.NumberOfAdults = bookingModel.NumberOfAdults;
            existingBooking.NumberOfChildren = bookingModel.NumberOfChildren;
            existingBooking.GuestName = bookingModel.GuestName ?? existingBooking.GuestName;
            existingBooking.GuestEmail = bookingModel.GuestEmail ?? existingBooking.GuestEmail;
            existingBooking.GuestPhoneNumber = bookingModel.GuestPhoneNumber ?? existingBooking.GuestPhoneNumber;
            existingBooking.GuestAddress = bookingModel.GuestAddress ?? existingBooking.GuestAddress;
            existingBooking.Notes = bookingModel.Notes;
            existingBooking.PaymentMethod = bookingModel.PaymentMethod ?? existingBooking.PaymentMethod;
            existingBooking.Status = bookingModel.Status;

            // Nếu trạng thái booking là "Đã thanh toán", cộng điểm thưởng cho người dùng
            if (existingBooking.Status == "Đã thanh toán")
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == existingBooking.UserId);
                if (user != null)
                {
                    // Tính điểm thưởng từ tổng tiền thanh toán
                    int rewardPoints = (int)(existingBooking.TotalPrice / 100000);  // Giả sử 1.000 VND = 1 điểm
                    user.RewardPoints += rewardPoints;
                    user.NumberOfToursTaken += 1;

                    // Cập nhật lại thông tin người dùng
                    _context.Users.Update(user);
                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges(); // Lưu lại booking và số chỗ còn lại của tour
        }

        public void Delete(string id)
        {
            var booking = _context.Bookings.Include(b => b.Tour).FirstOrDefault(b => b.Id == id);
            if (booking == null) return;

            var tour = booking.Tour;
            if (tour != null)
            {
                int totalPeople = booking.NumberOfAdults + booking.NumberOfChildren;
                tour.AvailableSlots += totalPeople; // Hoàn lại số chỗ cho tour
            }

             // Nếu số chỗ trống > 0, cập nhật trạng thái isActive về true
            if (!tour.IsActive && tour.AvailableSlots > 0)
            {
                tour.IsActive = true;
            }

            _context.Bookings.Remove(booking);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges(); // Lưu lại số chỗ đã được hoàn lại
        }

        public int GetBookingCount()
        {
            return _context.Bookings.Count(); // Đếm số lượng booking trong cơ sở dữ liệu
        }

        public decimal GetTotalRevenue()
        {
            // Tính tổng doanh thu từ các booking có trạng thái "Đã thanh toán"
            return _context.Bookings
                .Where(b => b.Status == "Đã thanh toán")
                .Sum(b => b.TotalPrice); // `Amount` là cột chứa giá trị doanh thu của mỗi booking
        }

        public List<MonthlyRevenueModel> GetMonthlyRevenue()
        {
            // Lấy năm hiện tại
            int currentYear = DateTime.Now.Year;

            // Lấy dữ liệu doanh thu từ các booking có trạng thái "Đã thanh toán"
            var monthlyRevenue = _context.Bookings
                .Where(b => b.Status == "Đã thanh toán" &&
                            b.BookingDate.Year == currentYear) // Lọc booking theo năm hiện tại
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month }) // Nhóm theo năm và tháng của ngày booking
                .Select(g => new MonthlyRevenueModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(b => b.TotalPrice) // Tổng doanh thu cho mỗi tháng
                })
                .ToList();

            // Tạo danh sách các tháng trong năm, với doanh thu mặc định là 0 cho mỗi tháng
            var allMonthsInYear = Enumerable.Range(1, 12).Select(month => new MonthlyRevenueModel
            {
                Year = currentYear, // Năm hiện tại
                Month = month,
                TotalRevenue = 0 // Doanh thu mặc định là 0
            }).ToList();

            // Ghép dữ liệu doanh thu từ cơ sở dữ liệu vào danh sách các tháng
            foreach (var revenue in monthlyRevenue)
            {
                var monthData = allMonthsInYear.FirstOrDefault(m => m.Month == revenue.Month);
                if (monthData != null)
                {
                    monthData.TotalRevenue = revenue.TotalRevenue;
                }
            }

            // Trả về danh sách các tháng với doanh thu
            return allMonthsInYear.OrderBy(m => m.Month).ToList();
        }


        public List<PopularCategoryModel> GetPopularCategories()
        {
            // Lấy danh sách các booking có trạng thái "Đã thanh toán"
            var paidBookings = _context.Bookings
                .Include(b => b.Tour) // Bao gồm thông tin về Tour
                .ThenInclude(t => t.Category) // Bao gồm thông tin về Category
                .Where(b => b.Status == "Đã thanh toán")
                .ToList();

            // Kiểm tra nếu không có booking nào thì trả về danh sách rỗng
            if (!paidBookings.Any())
            {
                return new List<PopularCategoryModel>();
            }

            // Tính toán số lượng người đến từng danh mục
            var categoryStats = paidBookings
                .GroupBy(b => b.Tour.Category.Name) // Nhóm theo danh mục
                .Select(group => new PopularCategoryModel
                {
                    Category = group.Key,
                    TotalVisitors = group.Sum(b => b.NumberOfAdults + b.NumberOfChildren) // Tổng số người trong danh mục này
                })
                .OrderByDescending(c => c.TotalVisitors) // Sắp xếp theo số lượng khách giảm dần
                .ToList();

            return categoryStats;
        }

        public List<PopularTourModel> GetPopularTours()
        {
            var paidBookings = _context.Bookings
                .Include(b => b.Tour)
                .Where(b => b.Status == "Đã thanh toán")
                .ToList();

            if (!paidBookings.Any())
            {
                return new List<PopularTourModel>();
            }

            var tourStats = paidBookings
                .GroupBy(b => b.Tour.Name) // Nhóm theo tên tour
                .Select(group => new PopularTourModel
                {
                    TourName = group.Key,
                    TotalVisitors = group.Sum(b => b.NumberOfAdults + b.NumberOfChildren) // Tổng số khách
                })
                .OrderByDescending(t => t.TotalVisitors)
                .ToList();

            return tourStats;
        }

    }
}
