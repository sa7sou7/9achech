using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models
{
    public enum ChecklistLibelle { PasserCommande, Recouvrement, ProduitConcurant }

    public class ChecklistRapport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VisitId { get; set; }

        [ForeignKey("VisitId")]
        public Visit Visit { get; set; }

        [Required]
        public ChecklistLibelle Libelle { get; set; }

        [MaxLength(500)]
        public string Commentaire { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        [RequiredIfRecouvrement(ErrorMessage = "ExpectedRecoveryAmount is required and must be greater than 0 for Recouvrement checklist.")]
        public decimal? ExpectedRecoveryAmount { get; set; } // Amount set by manager

        public decimal? RemainingRecoveryAmount { get; set; } // Remaining amount to collect
    }

    // Custom validation attribute
    public class RequiredIfRecouvrementAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var checklist = (ChecklistRapport)validationContext.ObjectInstance;
            if (checklist.Libelle == ChecklistLibelle.Recouvrement)
            {
                if (value == null || (decimal)value <= 0)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}