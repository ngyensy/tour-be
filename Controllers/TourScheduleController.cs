using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class TourScheduleController : ControllerBase
    {
        private readonly DataContext _context;

        // Tiêm DbContext vào controller
        public TourScheduleController(DataContext context)
        {
            _context = context;
        }

        // Lấy tất cả lịch khởi hành
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourSchedule>>> GetAllTourSchedules()
        {
            // Lấy danh sách các TourSchedule chỉ chứa TourID
            var tourSchedules = await _context.TourSchedules
                                            .Select(ts => new {
                                                ts.Id,
                                                ts.StartDate,
                                                ts.EndDate,
                                                ts.TourId // Chỉ lấy TourId từ bảng Tour
                                            })
                                            .ToListAsync();
            
            return Ok(tourSchedules);
        }

        // Lấy lịch khởi hành theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TourSchedule>> GetTourScheduleById(int id)
        {
            // Lấy thông tin TourSchedule theo ID mà không cần phải include Tour
            var tourSchedule = await _context.TourSchedules
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (tourSchedule == null)
            {
                return NotFound();
            }

            // Trả về chỉ thông tin của TourSchedule mà không có thông tin Tour
            return Ok(tourSchedule);
        }

        // Tạo mới lịch khởi hành
        [HttpPost]
        public async Task<ActionResult<TourSchedule>> CreateTourSchedule(TourSchedule tourSchedule)
        {
            if (tourSchedule == null)
            {
                return BadRequest();
            }

            // Tính toán ngày về (EndDate) dựa trên StartDate và Duration của tour
            var tour = await _context.Tours.FindAsync(tourSchedule.TourId);
            if (tour == null)
            {
                return NotFound("Tour không tồn tại.");
            }

            // Giả sử Duration là số ngày tour, bạn có thể tính toán EndDate
            tourSchedule.EndDate = tourSchedule.StartDate.AddDays(tour.Duration - 1);

            _context.TourSchedules.Add(tourSchedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTourScheduleById), new { id = tourSchedule.Id }, tourSchedule);
        }


        // Cập nhật lịch khởi hành
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTourSchedule(int id, TourSchedule tourSchedule)
        {
            if (id != tourSchedule.Id)
            {
                return BadRequest();
            }

            // Cập nhật EndDate khi ngày khởi hành thay đổi
            var tour = await _context.Tours.FindAsync(tourSchedule.TourId);
            if (tour == null)
            {
                return NotFound("Tour không tồn tại.");
            }

            tourSchedule.EndDate = tourSchedule.StartDate.AddDays(tour.Duration - 1);

            _context.Entry(tourSchedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TourSchedules.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // Xóa lịch khởi hành
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourSchedule(int id)
        {
            var tourSchedule = await _context.TourSchedules.FindAsync(id);
            if (tourSchedule == null)
            {
                return NotFound();
            }

            _context.TourSchedules.Remove(tourSchedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
    }
}
