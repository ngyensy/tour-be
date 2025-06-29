using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // Lấy tất cả đánh giá của một tour
        [HttpGet("tour/{tourId}")]
        public IActionResult GetReviewsByTour(string tourId)
        {
            var reviews = _reviewService.GetReviewsByTour(tourId);
            return Ok(reviews);
        }

        // Thêm đánh giá mới
        [HttpPost]
        public IActionResult AddReview([FromBody] Review review)
        {
            if (review == null || string.IsNullOrEmpty(review.TourId) || review.UserId <= 0 || review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest(new { message = "Dữ liệu đánh giá không hợp lệ." });
            }

            // Kiểm tra nếu người dùng đã hoàn thành tour
            if (!_reviewService.HasUserCompletedTour(review.TourId, review.UserId))
            {
                return BadRequest(new { message = "Bạn chỉ có thể đánh giá khi đã trải nghiệm Tour." });
            }

            var createdReview = _reviewService.AddReview(review);
            return CreatedAtAction(nameof(GetReviewsByTour), new { tourId = review.TourId }, createdReview);
        }

        // Sửa đánh giá
        [HttpPut("{id}")]
        public IActionResult UpdateReview(int id, [FromBody] Review review)
        {
            if (review == null || id <= 0)
            {
                return BadRequest(new { message = "Thông tin không hợp lệ." });
            }

            var updatedReview = _reviewService.UpdateReview(id, review);
            if (updatedReview == null)
            {
                return NotFound(new { message = "Đánh giá không tồn tại." });
            }

            return Ok(updatedReview);
        }

        // Xóa đánh giá
        [HttpDelete("{id}")]
        public IActionResult DeleteReview(int id)
        {
            var isDeleted = _reviewService.DeleteReview(id);
            if (!isDeleted)
            {
                return NotFound(new { message = "Đánh giá không tồn tại." });
            }

            return Ok(new { message = "Đánh giá đã được xóa." });
        }
    }
}
