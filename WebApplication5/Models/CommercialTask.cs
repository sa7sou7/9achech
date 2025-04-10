using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication5.Models
{
    public enum TaskType { Relance, Demonstration, Negociation }
    public enum TaskStatus { ToDo, InProgress, Completed }

    public class CommercialTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CommercialId { get; set; } // Foreign key to IdentityUser (Commercial)

        [Required]
        public TaskType Type { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [ForeignKey("CommercialId")]
        public IdentityUser Commercial { get; set; } // Navigation property
    }
}