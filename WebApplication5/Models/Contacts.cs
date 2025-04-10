using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models
{
    public class Contacts
    {

        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [ForeignKey("Tiers")]
        public int TiersId { get; set; }

        public Tiers Tiers { get; set; }  // Non-nullable
    }
}
