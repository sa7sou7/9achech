﻿namespace WebApplication5.Dto
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string RecipientId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? VisitId { get; set; } 

    }
}