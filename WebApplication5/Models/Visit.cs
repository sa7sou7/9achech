using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CommercialId { get; set; } = string.Empty;

        [Required]
        public int TiersId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string Report { get; set; } = string.Empty;

        public bool IsValidated { get; set; } = false;

        public List<VisitChecklist> Objectives { get; set; } = new List<VisitChecklist>();

        [ForeignKey("CommercialId")]
        public Commercial Commercial { get; set; }

        [ForeignKey("TiersId")]
        public Tiers Client { get; set; }
    }
}