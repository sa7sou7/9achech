using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string DocRef { get; set; } = string.Empty; // Maps to API "docref"

        public int TiersId { get; set; } // FK to Tiers

        [Required]
        [MaxLength(20)]
        public string DocRepresentant { get; set; } = string.Empty; // Maps to API "DocRepresentant"

        [Column(TypeName = "decimal(18,4)")]
        public decimal DocNetAPayer { get; set; } // Maps to API "DocNetaPayer"

        public DateTime DocDate { get; set; } // Maps to API "DocDate"

        [ForeignKey("TiersId")]
        public Tiers Client { get; set; } // Navigation property

    }


}
