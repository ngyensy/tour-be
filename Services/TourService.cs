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
    public interface ITourService
    {
        IEnumerable<TourModel> GetAll();
        TourModel GetById(string id);
        void Create(TourModel tour);
        void Update(string id, TourModel tour);
        void Delete(string id);
        Tour GetByIdWithItineraries(string id);
        int GetTourCount();
    }


    namespace WebApi.Services
    {
        public class TourService : ITourService
        {
            private DataContext _context;
            private readonly IMapper _mapper;

            public TourService(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public IEnumerable<TourModel> GetAll()
            {
                var tours = _context.Tours.ToList();
                return _mapper.Map<IEnumerable<TourModel>>(tours);
            }

            public Tour GetByIdWithItineraries(string id)
            {
                return _context.Tours
                    .Include(t => t.Itineraries) // Bao gồm thông tin lịch trình
                    .Include(t => t.Category)
                    .Include(t => t.TourSchedules) 
                    .FirstOrDefault(t => t.Id == id);
            }

            public TourModel GetById(string id)
            {
                var tour = _context.Tours.Find(id);
                return _mapper.Map<TourModel>(tour);
            }

            public void Create(TourModel tourModel)
            {
                try
    {
                // Kiểm tra dữ liệu đầu vào (nếu cần thêm logic)
                if (string.IsNullOrEmpty(tourModel.Name) || tourModel.Price <= 0 || tourModel.ChildPrice <= 0)
                {
                    throw new Exception("Invalid tour data.");
                }

                // Map từ TourModel sang thực thể Tour
                var tourEntity = _mapper.Map<Tour>(tourModel);

                // Gán Id cho tourEntity bằng chuỗi sinh ngẫu nhiên
                tourEntity.Id = IdGenerator.GenerateTourId();

                // Nếu có ảnh từ tourModel, xử lý ảnh và lưu đường dẫn (nếu có chức năng upload ảnh)
                if (!string.IsNullOrEmpty(tourModel.Image))
                {
                    // Giả sử bạn đã xử lý việc lưu ảnh trước đó và có đường dẫn ảnh
                    tourEntity.Image = tourModel.Image; // Gán đường dẫn ảnh
                }

                // Lưu tour vào cơ sở dữ liệu
                    _context.Tours.Add(tourEntity);
                    _context.SaveChanges();

                }
                catch (DbUpdateException ex)
                {
                    var innerException = ex.InnerException?.Message;
                    Console.WriteLine(innerException); // Xem chi tiết lỗi trong console
                    throw;
                }
            }


            public void Update(string id, TourModel tourModel)
            {
                var existingTour = _context.Tours.Find(id);
                if (existingTour == null) return;

                _mapper.Map(tourModel, existingTour);
                _context.Tours.Update(existingTour);
                _context.SaveChanges();
            }

            public void Delete(string id)
            {
                var tour = _context.Tours.Find(id);
                if (tour == null) return;

                _context.Tours.Remove(tour);
                _context.SaveChanges();
            }

            public int GetTourCount()
            {
                return _context.Tours.Count(); // Đếm số lượng tour trong cơ sở dữ liệu
            }
        }
    }

}
