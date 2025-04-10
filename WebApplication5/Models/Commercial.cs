using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models
{
    public class Commercial
    {

        [Key]
   
        public string Cref { get; set; } = string.Empty; // Maps to API "cref"

        [MaxLength(50)]
        public string Cnom { get; set; } = string.Empty; // Maps to API "cnom"

        [MaxLength(50)]
        public string CPrenom { get; set; } = string.Empty; // Maps to API "cprenom"

        public string UserId { get; set; } = string.Empty; // FK to IdentityUser

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; } // Navigation property
    }
}
