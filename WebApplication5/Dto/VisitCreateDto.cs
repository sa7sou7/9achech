using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Dtos
{
    public class VisitCreateDto
    {
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "TiersId is required.")]
        public int TiersId { get; set; }

        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "CommercialCref is required.")]
        public string CommercialCref { get; set; } = string.Empty;
    }
}