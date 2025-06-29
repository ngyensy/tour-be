using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using WebApi.Entities;

[Table("Tours")]
public class Tour
{
    [Key]
    [Column("Id")]
    public string Id { get; set; }

    [Column("Name")]
    public string Name { get; set; }

    [Column("Description")]
    public string Description { get; set; }

    [Column("Duration")]
    public int Duration { get; set; }

    [Column("Price")]
    public decimal Price { get; set; } // Giá cho người lớn

    [Column("ChildPrice")]
    public decimal ChildPrice { get; set; } // Giá cho trẻ em

    [Column("SingleRoomSurcharge")]
    public decimal SingleRoomSurcharge { get; set; }

    [Column("Discount")]
    public decimal? Discount { get; set; }

    [Column("DepartureLocation")]
    public string DepartureLocation { get; set; } // Nơi khởi hành

    [Column("Destination")]
    public string Destination { get; set; } // Điểm đến

    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime EndDate { get; set; }

    [Column("AvailableSlots")]
    public int AvailableSlots { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("Image")]
    public string Image { get; set; }

    [Column("CategoryId")]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; } // Navigation property

    public ICollection<Itinerary> Itineraries { get; set; } // 1-nhiều với Itinerary

    // Thêm quan hệ 1-nhiều với TourSchedule
    public ICollection<TourSchedule> TourSchedules { get; set; }
}
