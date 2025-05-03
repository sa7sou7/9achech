using System;
using System.Collections.Generic;
using WebApplication5.Models;

namespace WebApplication5.Dtos
{
    public class VisitCreateWithChecklistDto
    {
        public DateTime Date { get; set; }
        public int TiersId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string CommercialCref { get; set; }
        public List<ChecklistRapportCreateDto> Checklists { get; set; }
    }
}