using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace WebApplication5.Models
{
    public enum VisitStatus { Incompleted, Completed, Annuler }

    public class Visit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TiersId { get; set; }

        [ForeignKey("TiersId")]
        public Tiers Client { get; set; }

        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        [Required]
        public string CommercialCref { get; set; }

        [ForeignKey("CommercialCref")]
        public Commercial Commercial { get; set; }

        [Required]
        public VisitStatus Status { get; set; } = VisitStatus.Incompleted;

        public List<ChecklistRapport> Checklists { get; set; } = new List<ChecklistRapport>();
        public List<Recovery> Recoveries { get; set; } = new List<Recovery>();
        public List<CompetitorProduct> CompetitorProducts { get; set; } = new List<CompetitorProduct>();
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}