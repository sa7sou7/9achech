namespace WebApplication5.Dto
{
    public class CommercialTaskDto
    {
        public string CommercialId { get; set; }
        public string Type { get; set; } // Relance, Demonstration, Negociation
        public string Description { get; set; }
        public string Status { get; set; } // ToDo, InProgress, Completed
        public string? DueDate { get; set; }
    }

    public class CommercialTaskUpdateDto
    {
        public string Status { get; set; } // ToDo, InProgress, Completed
    }
}