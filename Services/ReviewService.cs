using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IReviewService
    {
        List<Review> GetReviewsByTour(string tourId);
        Review AddReview(Review review);
        Review UpdateReview(int id, Review review);
        bool DeleteReview(int id);
        bool HasUserCompletedTour(string tourId, int userId);
    }

    public class ReviewService : IReviewService
    {
        private readonly DataContext _context;

        public ReviewService(DataContext context)
        {
            _context = context;
        }

        public List<Review> GetReviewsByTour(string tourId)
        {
            return _context.Reviews
                .Where(r => r.TourId == tourId)
                .OrderByDescending(r => r.CreatedAt)
                .Include(r => r.User)  // Bao gồm thông tin người dùng
                .ToList();
        }


        public Review AddReview(Review review)
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            _context.SaveChanges();
            return review;
        }

        public Review UpdateReview(int id, Review review)
        {
            var existingReview = _context.Reviews.FirstOrDefault(r => r.Id == id);
            if (existingReview == null)
            {
                return null;
            }

            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Update(existingReview);
            _context.SaveChanges();
            return existingReview;
        }

        public bool DeleteReview(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.Id == id);
            if (review == null)
            {
                return false;
            }

            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return true;
        }

        public bool HasUserCompletedTour(string tourId, int userId)
        {
            // Kiểm tra trong lịch sử đặt tour
            return _context.Bookings.Any(b => b.TourId == tourId && b.UserId == userId && b.Status == "Đã thanh toán");
        }
    }
}
