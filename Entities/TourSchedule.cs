using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using WebApi.Entities;

[Table("TourSchedules")]
public class TourSchedule
{
    [Key]
    public int Id { get; set; } // ID của lịch khởi hành

    [Column("TourId")]
    public string TourId { get; set; } // Khóa ngoại liên kết với Tour

    [ForeignKey("TourId")]
    public Tour Tour { get; set; } // Navigation property

    [Column("StartDate")]
    public DateTime StartDate { get; set; } // Ngày khởi hành

    [Column("EndDate")]
    public DateTime EndDate { get; set; }    // Ngày về

}
