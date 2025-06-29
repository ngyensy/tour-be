using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApi.Entities;

namespace WebApi.Models
{
    public class CategoryModel
    {
        public int Id { get; set; } // ID của category
        public string Name { get; set; } // Tên của category
        public string Description { get; set; } // Mô tả của category
        public string Image { get; set; }

        public ICollection<Tour> Tour { get; set; }
    }
}
