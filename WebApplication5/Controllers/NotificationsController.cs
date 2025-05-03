using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication5.Dto;
using WebApplication5.Repository;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserMappingService _userMappingService;

        public NotificationsController(
            INotificationRepository notificationRepository,
            UserMappingService userMappingService)
        {
            _notificationRepository = notificationRepository;
            _userMappingService = userMappingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var notifications = await _notificationRepository.GetByRecipientAsync(userId);
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                RecipientId = n.RecipientId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                VisitId = n.VisitId
            });
            return Ok(notificationDtos);
        }

        [HttpGet("by-cref/{commercialCref}")]
        public async Task<IActionResult> GetNotificationsByCommercialCref(string commercialCref)
        {
            if (string.IsNullOrWhiteSpace(commercialCref))
            {
                return BadRequest("CommercialCref is required.");
            }

            try
            {
                var userId = await _userMappingService.GetUserIdFromCrefAsync(commercialCref);
                var notifications = await _notificationRepository.GetByRecipientAsync(userId);
                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    RecipientId = n.RecipientId,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    VisitId = n.VisitId
                });
                return Ok(notificationDtos);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to retrieve notifications: {ex.Message}");
            }
        }

        [HttpPut("{id}/read")]
        [AllowAnonymous] // Explicitly allow anonymous access
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            await _notificationRepository.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}