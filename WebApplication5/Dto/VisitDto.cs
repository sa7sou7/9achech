using System;
using System.Collections.Generic;
using WebApplication5.Dto;
using WebApplication5.Models;

namespace WebApplication5.Dtos
{
    public class VisitDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TiersId { get; set; }
        public string ClientNom { get; set; }
        public string Note { get; set; }
        public string CommercialCref { get; set; }
        public string CommercialNom { get; set; }
        public VisitStatus Status { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public List<ChecklistRapportDto> Checklists { get; set; }
        public List<RecoveryDto> Recoveries { get; set; }
        public List<CompetitorProductDto> CompetingProducts { get; set; }
        public List<OrderDto> Orders { get; set; }
    }
}