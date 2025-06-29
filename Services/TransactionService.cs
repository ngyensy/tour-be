using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface ITransactionService
    {
        IEnumerable<TransactionModel> GetAllTransactions();
        TransactionModel GetTransactionById(string id);
        void CreateTransaction(TransactionModel model);
        void UpdateTransaction(string id, TransactionModel model);
        void DeleteTransaction(string id);
    }

    public class TransactionService : ITransactionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public TransactionService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<TransactionModel> GetAllTransactions()
        {
            var transactions = _context.Transactions
                .Include(t => t.Booking)
                .ToList();

            return _mapper.Map<IEnumerable<TransactionModel>>(transactions);
        }

        public TransactionModel GetTransactionById(string id)
        {
            var transaction = _context.Transactions
                .Include(t => t.Booking)
                .FirstOrDefault(t => t.Id == id);

            return _mapper.Map<TransactionModel>(transaction);
        }

        public void CreateTransaction(TransactionModel model)
        {
            var transactionEntity = _mapper.Map<Transaction>(model);

            // Tạo ID ngẫu nhiên cho giao dịch
            transactionEntity.Id = IdGenerator.GenerateTransactionId(10);
            transactionEntity.TransactionDate = DateTime.Now;

            _context.Transactions.Add(transactionEntity);
            _context.SaveChanges();
        }

        public void UpdateTransaction(string id, TransactionModel model)
        {
            var transaction = _context.Transactions
                .Include(t => t.Booking) // Bao gồm thông tin booking
                .FirstOrDefault(t => t.Id == id);

            if (transaction == null)
                throw new KeyNotFoundException("Transaction not found");

            // Kiểm tra nếu trạng thái thay đổi thành "Đã hủy giao dịch"
            if (model.Status == "Đã hủy giao dịch" && transaction.Booking != null)
            {
                // Kiểm tra trạng thái của booking trước khi cập nhật
                if (transaction.Booking.Status != "Đã Hủy Booking")
                {
                    transaction.Booking.Status = "Đã Hủy Booking"; // Cập nhật trạng thái của Booking
                }
            }

            if (model.Status == "Đã hoàn thành" && transaction.Booking != null)
            {
                // Kiểm tra trạng thái của booking trước khi cập nhật
                if (transaction.Booking.Status != "Đã thanh toán")
                {
                    transaction.Booking.Status = "Đã thanh toán"; // Cập nhật trạng thái của Booking
                }
            }

            if (model.Status == "Đã đặt cọc" && transaction.Booking != null)
            {
                // Kiểm tra trạng thái của booking trước khi cập nhật
                if (transaction.Booking.Status != "Đã xác nhận")
                {
                    transaction.Booking.Status = "Đã xác nhận"; // Cập nhật trạng thái của Booking
                }
            }

            // Cập nhật các thuộc tính có giá trị trong model
            if (!string.IsNullOrEmpty(model.Status))
            {
                transaction.Status = model.Status;
            }

            if (!string.IsNullOrEmpty(model.PaymentMethod))
            {
                transaction.PaymentMethod = model.PaymentMethod;
            }

            // Lưu lại thay đổi
            _context.SaveChanges();
        }


        public void DeleteTransaction(string id)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
                throw new KeyNotFoundException("Transaction not found");

            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
        }
    }

}
