using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PodcastWebApp.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UnreadCount()
        {
            // Mock implementation - in real app would check database
            return Json(new { count = 0 });
        }
    }
}