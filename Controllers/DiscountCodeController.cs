using Microsoft.AspNetCore.Mvc;
using System;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class DiscountCodeController : ControllerBase
    {
        private readonly IDiscountCodeService _discountCodeService;

        public DiscountCodeController(IDiscountCodeService discountCodeService)
        {
            _discountCodeService = discountCodeService;
        }

        [HttpGet]
        public IActionResult GetAllDiscountCodes()
        {
            var discountCodes = _discountCodeService.GetAllDiscountCodes();
            return Ok(discountCodes);
        }

        [HttpGet("{id}")]
        public IActionResult GetDiscountCodeById(int id)
        {
            var discountCode = _discountCodeService.GetDiscountCodeById(id);
            if (discountCode == null)
                return NotFound(new { message = "Mã giảm giá không tồn tại." });

            return Ok(discountCode);
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetDiscountCodesByUser(int userId)
        {
            try
            {
                var discountCodes = _discountCodeService.GetDiscountCodesByUser(userId);
                if (discountCodes == null || discountCodes.Count == 0)
                {
                    return NotFound(new { message = "Không có mã giảm giá cho người dùng này." });
                }
                
                return Ok(discountCodes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi lấy mã giảm giá: {ex.Message}" });
            }
        }


        [HttpPost]
        public IActionResult CreateDiscountCode([FromBody] DiscountCode discountCode)
        {
            if (string.IsNullOrEmpty(discountCode.Code))
            {
                discountCode.Code = "DC-" + Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper();
            }

            var createdCode = _discountCodeService.CreateDiscountCode(discountCode);
            return CreatedAtAction(nameof(GetDiscountCodeById), new { id = createdCode.Id }, createdCode);
        }

        [HttpPost("redeem")]
        public IActionResult RedeemPoints([FromBody] RedeemDiscountRequest request)
        {
            if (request == null || request.UserId <= 0 || request.RewardPointsRequired <= 0)
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            try
            {
                var discountCode = _discountCodeService.RedeemPoints(request.UserId, request.RewardPointsRequired, request.AmountDiscount);
                return Ok(new { Code = discountCode.Code });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Phương thức xóa mã giảm giá
        [HttpDelete("{id}")]
        public IActionResult DeleteDiscountCode(int id)
        {
            try
            {
                var isDeleted = _discountCodeService.DeleteDiscountCode(id);
                if (!isDeleted)
                {
                    return NotFound(new { message = "Mã giảm giá không tồn tại hoặc không thể xóa." });
                }

                // Trả về thông báo thành công khi xóa
                return Ok(new { message = "Mã giảm giá đã được xóa thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi xóa mã giảm giá: {ex.Message}" });
            }
        }
        
    
        [HttpPost("validate")]
        public IActionResult ValidateDiscountCode([FromBody] ValidateDiscountCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { message = "Mã giảm giá không được để trống." });
            }

            try
            {
                // Kiểm tra tính hợp lệ của mã giảm giá
                if (_discountCodeService.ValidateDiscountCode(request.Code, out var discountCode))
                {
                    // Mã giảm giá hợp lệ, trả về thông tin
                    return Ok(new 
                    { 
                        message = "Mã giảm giá hợp lệ.", 
                        discountAmount = discountCode.AmountDiscount  // Trả về số tiền giảm
                    });
                }
                else
                {
                    return BadRequest(new { message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi xác thực mã giảm giá: {ex.Message}" });
            }
        }


        public class RedeemDiscountRequest
        {
            public int UserId { get; set; }
            public int RewardPointsRequired { get; set; }
            public double AmountDiscount { get; set;}
        }

        public class ValidateDiscountCodeRequest
        {
            public string Code { get; set; }  // Mã giảm giá
        }
    }
}
