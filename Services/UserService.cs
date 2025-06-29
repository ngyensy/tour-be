using AutoMapper;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using System.Net.Mail;
using StackExchange.Redis;
using System;

namespace WebApi.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Create(CreateRequest model);
        void Update(int id, UpdateRequest model);
        void Delete(int id);
        User Authenticate(string email, string password);
        void UpdateAvatar(int userId, string avatarUrl);
        int GetUserCount();
        void UpdateRefreshToken(int userId, string refreshToken);
        public void ChangePassword(int id, string oldPassword, string newPassword);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConnectionMultiplexer _redis;

        public UserService(
            DataContext context,
            IMapper mapper,
            IConnectionMultiplexer redis
            )
        {
            _context = context;
            _mapper = mapper;
            _redis = redis;

        }

        public User Authenticate(string username, string password)
        {
            // Kiểm tra xem username là email hay số điện thoại
            var user = _context.Users.SingleOrDefault(x =>
                x.Email == username || x.PhoneNumber == username);

            // Nếu không tìm thấy người dùng
            if (user == null)
                throw new AppException("Tên đăng nhập không tồn tại");

            // Nếu không tìm thấy hoặc mật khẩu không đúng
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
                return null;

            return user;
        }
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Kiểm tra mật khẩu với hash trong database
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to validate phone number (10 digits)
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10;
        }

        public IEnumerable<User> GetAll()
            {
                return _context.Users;
            }

            public User GetById(int id)
            {
                return getUser(id);
            }

            public void Create(CreateRequest model)
            {
                // validate
                if (_context.Users.Any(x => x.Email == model.Email))
                    throw new AppException("email '" + model.Email + "' đã tồn tại");

                  // validate password
                if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
                    throw new AppException("Mật khẩu phải có ít nhất 8 ký tự");
                
                // validate phone number (10 digits)
                if (!IsValidPhoneNumber(model.PhoneNumber))
                    throw new AppException("Số điện thoại phải có đúng 10 chữ số");

                // map model to new user object
                var user = _mapper.Map<User>(model);

                // hash password
                user.PasswordHash = BCryptNet.HashPassword(model.Password);

                // save user
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            public void Update(int id, UpdateRequest model)
            {
                var user = getUser(id);

                // validate
                if (model.Email != user.Email && _context.Users.Any(x => x.Email == model.Email))
                    throw new AppException("User with the email '" + model.Email + "' already exists");

                // hash password if it was entered
                if (!string.IsNullOrEmpty(model.Password))
                    user.PasswordHash = BCryptNet.HashPassword(model.Password);

                // copy model to user and save
                _mapper.Map(model, user);
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            public void ChangePassword(int id, string oldPassword, string newPassword)
            {
                var user = getUser(id);

                // Kiểm tra mật khẩu cũ
                if (!VerifyPasswordHash(oldPassword, user.PasswordHash))
                    throw new AppException("Mật khẩu cũ không đúng");

                // Validate mật khẩu mới (ví dụ: không rỗng, đủ độ dài)
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                    throw new AppException("Mật khẩu phải có ít nhất 8 ký tự");

                // Hash mật khẩu mới
                user.PasswordHash = BCryptNet.HashPassword(newPassword);

                // Cập nhật người dùng trong cơ sở dữ liệu
                _context.Users.Update(user);
                _context.SaveChanges();
            }


        public void Delete(int id)
            {
                var user = getUser(id);
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            // helper methods

            private User getUser(int id)
            {
                var user = _context.Users.Find(id);
                if (user == null) throw new KeyNotFoundException("User not found");
                return user;
            }

            public void UpdateAvatar(int userId, string avatarUrl)
            {
                var user = _context.Users.Find(userId);
                if (user == null) throw new KeyNotFoundException("User not found");

                user.Avatar = avatarUrl;
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            public int GetUserCount()
            {
                return _context.Users.Count(); // Đếm số lượng người dùng trong cơ sở dữ liệu
            }

            public void UpdateRefreshToken(int userId, string refreshToken)
            {
                var db = _redis.GetDatabase();
                db.StringSet($"refresh:{userId}", refreshToken, TimeSpan.FromDays(7)); // Lưu refresh token trong Redis với thời gian sống 7 ngày
            }

    }

}