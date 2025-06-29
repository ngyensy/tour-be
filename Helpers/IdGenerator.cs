using System;
using System.Linq;

namespace WebApi.Helpers
{
    public static class IdGenerator
    {
        public static string GenerateTourId()
        {
            string prefix = "NNSGN"; // Tiền tố cố định
            string randomNumber = new Random().Next(1000, 9999).ToString(); // Tạo số ngẫu nhiên 4 chữ số
            string datePart = DateTime.Now.ToString("yyMMdd"); // Lấy ngày tháng năm dạng YYMMDD
            string suffix = "VN-D"; // Hậu tố cố định

            return $"{prefix}{randomNumber}-{datePart}{suffix}";
        }

        public static string GenerateBookingId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal static string GenerateTransactionId(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than zero.", nameof(length));

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }

}
