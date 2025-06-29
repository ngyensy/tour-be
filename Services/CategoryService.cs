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
    public interface ICategoryService
    {
        IEnumerable<CategoryModel> GetAll();
        CategoryModel GetById(int id);
        void Create(CategoryModel category);
        void Update(int id, CategoryModel category);
        void Delete(int id);
    }

    namespace WebApi.Services
    {
        public class CategoryService : ICategoryService
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public CategoryService(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public IEnumerable<CategoryModel> GetAll()
            {
                var categories = _context.Categories.AsNoTracking().ToList();
                return _mapper.Map<IEnumerable<CategoryModel>>(categories);
            }

            public CategoryModel GetById(int id)
            {
                var category = _context.Categories.AsNoTracking().FirstOrDefault(c => c.Id == id);
                return _mapper.Map<CategoryModel>(category);
            }

            public void Create(CategoryModel categoryModel)
            {
                // Kiểm tra xem tên danh mục đã tồn tại hay chưa, so sánh không phân biệt chữ hoa chữ thường
                var existingCategory = _context.Categories
                    .FirstOrDefault(c => c.Name.ToLower() == categoryModel.Name.ToLower());

                if (existingCategory != null)
                {
                    // Nếu tên danh mục đã tồn tại
                    throw new InvalidOperationException("Tên danh mục đã tồn tại!");
                }

                // Nếu tên danh mục chưa tồn tại, tiến hành thêm mới danh mục
                var categoryEntity = _mapper.Map<Category>(categoryModel);
                _context.Categories.Add(categoryEntity);
                _context.SaveChanges();
            }


            public void Update(int id, CategoryModel categoryModel)
            {
                try
                {
                    // Kiểm tra xem tên danh mục đã tồn tại hay chưa, so sánh không phân biệt chữ hoa chữ thường
                    var existingCategoryByName = _context.Categories
                        .FirstOrDefault(c => c.Name.ToLower() == categoryModel.Name.ToLower() && c.Id != id);

                    if (existingCategoryByName != null)
                    {
                        // Nếu tên danh mục đã tồn tại và không phải là danh mục đang được chỉnh sửa
                        throw new InvalidOperationException("Tên danh mục đã tồn tại.");
                    }

                    // Tiến hành cập nhật danh mục nếu không có lỗi
                    var existingCategory = _context.Categories.Find(id);
                    if (existingCategory == null)
                    {
                        throw new KeyNotFoundException("Danh mục không tồn tại.");
                    }

                    existingCategory.Name = categoryModel.Name;
                    existingCategory.Description = categoryModel.Description;

                    // Cập nhật các thuộc tính khác nếu cần

                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Log the exception (hoặc in ra console để kiểm tra)
                    Console.WriteLine(ex.InnerException?.Message);
                    throw; // Ném lại lỗi sau khi log
                }
            }

            public void Delete(int id)
            {
                var category = _context.Categories.Find(id);
                if (category == null) return;

                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }
    }
}
