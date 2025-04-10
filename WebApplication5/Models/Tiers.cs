using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication5.Models
{

    public enum StatutProspect { Nouveau, EnCours, Converti, Abandonne, Client }

    public class Tiers
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Matricule { get; set; } = string.Empty; // Maps to API "code"

        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty; // Maps to API "rs"

        [MaxLength(200)]
        public string Adresse { get; set; } = string.Empty; // Maps to API "adresse"

        [MaxLength(100)]
        public string? Ville { get; set; } // Maps to API "ville"

        [MaxLength(100)]
        public string? Delegation { get; set; } // Maps to API "delegation"

        [MaxLength(50)]
        public string? SecteurActiv { get; set; } // Maps to API "SecteurActiv"

        [MaxLength(20)]
        public string? Tel { get; set; } // Maps to API "Tel"

        public string Cin { get; set; } = string.Empty; // Not in API

        public StatutProspect Statut { get; set; } = StatutProspect.Nouveau;

        [JsonIgnore]
        public List<Contacts> Contacts { get; set; } = new();
    }
   
}
