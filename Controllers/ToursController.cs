using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using System;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly IWebHostEnvironment _environment;

        public ToursController(ITourService tourService, IWebHostEnvironment environment)
        {
            _tourService = tourService;
            _environment = environment;
        }

        // GET: v1/tours
        [HttpGet]
        public IActionResult GetAll()
        {
            var tours = _tourService.GetAll();
            return Ok(tours);
        }

        // GET: v1/tours/5
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var tour = _tourService.GetByIdWithItineraries(id); // Gọi dịch vụ lấy tour kèm lịch trình
            if (tour == null)
                return NotFound();

            return Ok(tour); // Trả về thông tin tour kèm lịch trình
        }

        // POST: v1/tours
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TourModel tourModel, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Xử lý lưu ảnh
            if (image != null && image.Length > 0)
            {
                var imagePath = await SaveImage(image);
                if (imagePath == null) // Kiểm tra nếu lưu ảnh thất bại
                    return BadRequest(new { message = "Error saving image." });

                tourModel.Image = imagePath; // Gán đường dẫn ảnh vào model
            }

            _tourService.Create(tourModel);
            return Ok(new { message = "Tour created successfully." });
        }

        // PUT: v1/tours/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] TourModel tourModel, IFormFile image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Xử lý lưu ảnh mới nếu có
            if (image != null && image.Length > 0)
            {
                var imagePath = await SaveImage(image);
                if (imagePath == null) // Kiểm tra nếu lưu ảnh thất bại
                    return BadRequest(new { message = "Error saving image." });

                tourModel.Image = imagePath; // Cập nhật đường dẫn ảnh
            }

            _tourService.Update(id, tourModel);
            return Ok(new { message = "Đã cập nhật Tour thành công!!!" });
        }

        // DELETE: v1/tours/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _tourService.Delete(id);
            return Ok(new { message = "Đã xóa Tour thành công!!!" });
        }

        // Phương thức hỗ trợ lưu ảnh vào thư mục wwwroot/uploads
        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Trả về null hoặc thông báo lỗi
                return null; // Hoặc có thể trả về một thông điệp khác như "Error saving image"
            }

            return $"/uploads/{uniqueFileName}";

            
        }

        [HttpGet("count")]
        public IActionResult GetTourCount()
        {
            var count = _tourService.GetTourCount(); // Hàm này sẽ trả về số lượng tour
            return Ok(new { count });
        }

    }
}
