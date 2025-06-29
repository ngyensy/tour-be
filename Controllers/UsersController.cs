using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System;
using WebApi.Models.Users;
using WebApi.Services;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;

        public UsersController(
            IUserService userService,
            IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPost]
        public IActionResult Create(CreateRequest model)
        {
            _userService.Create(model);
            return Ok(new { message = "User created" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromForm] UpdateRequest model, [FromForm] IFormFile avatar = null)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var existingUser = _userService.GetById(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Cập nhật thông tin người dùng
                _userService.Update(id, model);

                // Nếu có tệp avatar được tải lên, xử lý việc lưu trữ
                if (avatar != null && avatar.Length > 0)
                {
                    // Đường dẫn thư mục lưu trữ avatar mới
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Tạo tên file mới để tránh trùng lặp
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Lưu file lên server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        avatar.CopyTo(stream);
                    }

                    // Cập nhật đường dẫn avatar vào cơ sở dữ liệu (trong User)
                    var avatarUrl = $"/avatars/{fileName}";
                    _userService.UpdateAvatar(id, avatarUrl);
                }

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPut("{id}/change-password")]
        public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Gọi phương thức ChangePassword từ IUserService
                _userService.ChangePassword(id, request.OldPassword, request.NewPassword); // Truyền mật khẩu cũ và mới
                return Ok(new { message = "Mật khẩu đã được đổi thành công!" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok(new { message = "User deleted" });
        }

        [HttpDelete("{id}/avatar")]
        public IActionResult DeleteAvatar(int id)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var existingUser = _userService.GetById(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Đường dẫn của avatar cần xóa
                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingUser.Avatar.TrimStart('/'));

                // Kiểm tra và xóa file avatar nếu tồn tại
                if (System.IO.File.Exists(avatarPath))
                {
                    System.IO.File.Delete(avatarPath); // Xóa tệp avatar trên server
                }

                // Cập nhật đường dẫn avatar trong cơ sở dữ liệu (nếu cần)
                _userService.UpdateAvatar(id, null); // Xóa avatar trong cơ sở dữ liệu (nếu bạn muốn lưu thông tin này)

                return Ok(new { message = "Avatar deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("count")]
        public IActionResult GetUserCount()
        {
            var count = _userService.GetUserCount(); // Hàm này sẽ trả về số lượng người dùng
            return Ok(new { count });
        }
    }
}
