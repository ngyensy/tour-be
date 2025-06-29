using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // ID tự động tăng

        [Required]
        public string TourId { get; set; } // ID của tour được đánh giá

        [Required]
        public int UserId { get; set; } // ID của người dùng đánh giá

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } // Điểm đánh giá (1-5 sao)

        public string Comment { get; set; } // Nội dung bình luận   

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thời gian tạo đánh giá

        public DateTime? UpdatedAt { get; set; } // Thời gian chỉnh sửa đánh giá

        // Navigation properties
        [ForeignKey("TourId")]
        public virtual Tour Tour { get; set; } // Liên kết với bảng Tour

        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Liên kết với bảng User
    }
}
