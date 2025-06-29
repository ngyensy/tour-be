using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
    [Table("DiscountCodes")]
    public class DiscountCode
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Code")]
        [MaxLength(50)]
        public string Code { get; set; }

        // Sử dụng AmountDiscount thay vì DiscountPercentage để giảm số tiền cố định
        [Column("AmountDiscount")]
        [Range(0, double.MaxValue)]
        public double AmountDiscount { get; set; } //so tien giam

        [Column("RewardPointsRequired")]    // diem can de doi
        [Range(0, int.MaxValue)]
        public int RewardPointsRequired { get; set; }

        [Column("ExpiryDate")]
        public DateTime ExpiryDate { get; set; } //Han su dung

        [Column("IsUsed")]
        public bool IsUsed { get; set; } = false;

        [ForeignKey("User")]
        [Column("UserId")]
        public int? UserId { get; set; }
        public User User { get; set; }
    }
}
