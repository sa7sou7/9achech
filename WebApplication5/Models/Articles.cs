using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication5.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty; // Fixed spelling

        [MaxLength(50)]
        public string? Famille { get; set; }

        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal PrixAchat { get; set; }

        [Precision(18, 2)]
        [Range(0, double.MaxValue)]
        public decimal PrixVente { get; set; }

        public int StockQuantity { get; set; }
    }
}