using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IDiscountCodeService
    {
        DiscountCode CreateDiscountCode(DiscountCode discountCode);
        DiscountCode RedeemPoints(int userId, int rewardPointsRequired, double amountDiscount);
        DiscountCode GetDiscountCodeById(int id);
        IEnumerable<DiscountCode> GetAllDiscountCodes();

        bool DeleteDiscountCode(int id);

        List<DiscountCode> GetDiscountCodesByUser(int userId);

        bool ValidateDiscountCode(string code, out DiscountCode discountCode);
    }

    public class DiscountCodeService : IDiscountCodeService
    {
        private readonly DataContext _context;

        public DiscountCodeService(DataContext context)
        {
            _context = context;
        }

        public DiscountCode CreateDiscountCode(DiscountCode discountCode)
        {
            if (string.IsNullOrEmpty(discountCode.Code))
            {
                discountCode.Code = GenerateCode(); // Tạo mã tự động nếu không được cung cấp
            }
            discountCode.ExpiryDate = DateTime.UtcNow.AddMonths(1);
            _context.DiscountCodes.Add(discountCode);
            _context.SaveChanges();
            return discountCode;
        }

        public DiscountCode RedeemPoints(int userId, int rewardPointsRequired, double amountDiscount)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new AppException("Người dùng không tồn tại.");
            if (user.RewardPoints < rewardPointsRequired) throw new AppException("Không đủ điểm thưởng.");

            var discountCode = new DiscountCode
            {
                Code = GenerateRandomCode(), // Tạo mã giảm giá tự động
                AmountDiscount = amountDiscount,
                RewardPointsRequired = rewardPointsRequired,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            user.RewardPoints -= rewardPointsRequired;
            _context.DiscountCodes.Add(discountCode);
            _context.Users.Update(user);
            _context.SaveChanges();

            return discountCode;
        }

        private string GenerateRandomCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }


        private string GenerateCode()
        {
            return $"DC-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"; // Tạo mã giảm giá ngẫu nhiên
        }


        public DiscountCode GetDiscountCodeById(int id)
        {
            return _context.DiscountCodes.FirstOrDefault(dc => dc.Id == id);
        }

        public IEnumerable<DiscountCode> GetAllDiscountCodes()
        {
            return _context.DiscountCodes.ToList();
        }

         public bool DeleteDiscountCode(int id)
        {
            var discountCode = _context.DiscountCodes.Find(id);
            if (discountCode == null)
            {
                return false; // Không tìm thấy mã giảm giá để xóa
            }

            _context.DiscountCodes.Remove(discountCode);
            _context.SaveChanges();
            return true; // Xóa thành công
        }

        public List<DiscountCode> GetDiscountCodesByUser(int userId)
        {
            // Lọc và xóa các mã giảm giá đã hết hạn
            var expiredCodes = _context.DiscountCodes
                .Where(dc => dc.UserId == userId && dc.ExpiryDate < DateTime.UtcNow)
                .ToList();

            if (expiredCodes.Any())
            {
                _context.DiscountCodes.RemoveRange(expiredCodes);
                _context.SaveChanges();
            }

            // Trả về danh sách mã giảm giá hợp lệ
            return _context.DiscountCodes
                .Where(dc => dc.UserId == userId && dc.ExpiryDate >= DateTime.UtcNow)
                .ToList();
        }

        public bool ValidateDiscountCode(string code, out DiscountCode discountCode)
        {
            // Lọc và xóa mã giảm giá hết hạn
            var expiredCodes = _context.DiscountCodes
                .Where(dc => dc.ExpiryDate < DateTime.UtcNow)
                .ToList();

            if (expiredCodes.Any())
            {
                _context.DiscountCodes.RemoveRange(expiredCodes);
                _context.SaveChanges();
            }

            // Kiểm tra mã giảm giá hợp lệ
            discountCode = _context.DiscountCodes.FirstOrDefault(dc => dc.Code == code);

            return discountCode != null && discountCode.ExpiryDate >= DateTime.UtcNow && !discountCode.IsUsed;
        }

    }
}