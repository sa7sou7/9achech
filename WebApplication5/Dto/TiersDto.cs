using WebApplication5.Models;

namespace WebApplication5.Dto
{
    public class TiersDto
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Matricule { get; set; }
        public string Cin { get; set; }
        public StatutProspect Statut { get; set; }
        public string? Ville { get; set; } // Add Ville
        public string? Delegation { get; set; } // Add Delegation
        public string? SecteurActiv { get; set; } // Add SecteurActiv
        public string? Tel { get; set; } // Add Tel
        public List<ContactsDto> Contacts { get; set; } = new();

    }
}
