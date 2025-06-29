using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingController(IBookingService bookingService, IHttpContextAccessor httpContextAccessor)
        {
            _bookingService = bookingService;
            _httpContextAccessor = httpContextAccessor;

        }

        

        // GET: api/booking
        [HttpGet]
        public IActionResult GetAll()
        {
            var bookings = _bookingService.GetAllBookings();
            if (bookings == null || !bookings.Any())
                return NotFound();

            return Ok(bookings);
        }
        
        // GET: api/booking/5
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var booking = _bookingService.GetById(id);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        // POST: api/booking
        [HttpPost]
        public IActionResult Create([FromBody] BookingModel bookingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra xem người dùng đã đăng nhập chưa
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                // Nếu người dùng đã đăng nhập, cập nhật thông tin booking
                bookingModel.UserId = int.Parse(userId); // Chuyển đổi ID người dùng thành int
            }

            // Tạo booking
            _bookingService.Create(bookingModel);

            return Ok(new { message = "Đã tạo booking thành công!!!" });
        }


        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] BookingModel bookingModel)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingBooking = _bookingService.GetById(id);
            if (existingBooking == null)
            {
                return NotFound(new { message = "Booking not found." });
            }

            _bookingService.Update(id, bookingModel);
            return Ok(new { message = "Đã cập nhật booking thành công!!!" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var existingBooking = _bookingService.GetById(id);
            if (existingBooking == null)
            {
                return NotFound(new { message = "không tìm thấy booking nào" });
            }

            _bookingService.Delete(id);
            return Ok(new { message = "đã xóa Booking thành công!!!" });
        }

        [HttpGet("count")]
        public IActionResult GetBookingCount()
        {
            var count = _bookingService.GetBookingCount(); // Hàm này sẽ trả về số lượng booking
            return Ok(new { count });
        }

        [HttpGet("revenue")]
        public IActionResult GetTotalRevenue()
        {
            try
            {
                var totalRevenue = _bookingService.GetTotalRevenue();
                return Ok(new { totalRevenue });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi khi tính tổng doanh thu.", error = ex.Message });
            }
        }

        [HttpGet("monthly-revenue")]
        public IActionResult GetMonthlyRevenue()
        {
            try
            {
                var monthlyRevenue = _bookingService.GetMonthlyRevenue();
                return Ok(monthlyRevenue);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã xảy ra lỗi khi lấy doanh thu hàng tháng.", error = ex.Message });
            }
        }

        // API để lấy danh sách các điểm đến yêu thích
        [HttpGet("popular-categories")]
        public ActionResult<List<PopularCategoryModel>> GetPopularCategories()
        {
            try
            {
                var popularCategories = _bookingService.GetPopularCategories();
                return Ok(popularCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.", error = ex.Message });
            }
        }

                [HttpGet("popular-tours")]
        public ActionResult<List<PopularTourModel>> GetPopularTours()
        {
            try
            {
                var popularTours = _bookingService.GetPopularTours();
                return Ok(popularTours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.", error = ex.Message });
            }
        }

    }
}
