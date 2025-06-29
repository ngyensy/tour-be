using System;
using System.ComponentModel.DataAnnotations;
using WebApi.Models.Users;

namespace WebApi.Models
{
    public class MonthlyRevenueModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
}
}
