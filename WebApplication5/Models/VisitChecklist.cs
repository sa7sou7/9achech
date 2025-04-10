using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models
{
    public enum VisitObjectiveType
    {
        PasserCommande,
        Recouvrement,
        Livraison,
        Promotion,
        Enquete,
        EssaiProduit
    }

    public class VisitChecklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VisitId { get; set; }

        [Required]
        public VisitObjectiveType ObjectiveType { get; set; }

        public bool IsSelected { get; set; } = false;

        // Fields specific to Recouvrement objective
        public decimal? Encours { get; set; }
        public bool? ReglementPret { get; set; }
        public bool? Recupere { get; set; }

        // Order items for PasserCommande objective
        public List<VisitOrderItem> OrderItems { get; set; } = new List<VisitOrderItem>();

        [ForeignKey("VisitId")]
        public Visit Visit { get; set; }
    }

    public class VisitOrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VisitChecklistId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal UnitPriceHT { get; set; }

        [Precision(18, 2)]
        public decimal Discount { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int AvailableQuantity { get; set; }

        [ForeignKey("VisitChecklistId")]
        public VisitChecklist VisitChecklist { get; set; }

        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
    }
}