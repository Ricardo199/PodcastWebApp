/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;
using PodcastWebApp.Models;

namespace PodcastWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext database;

        public SubscriptionsController(ApplicationDbContext db)
        {
            database = db;
        }

        // Subscribe to podcast
        [HttpPost]
        public async Task<IActionResult> Subscribe(string userId, int podcastId)
        {
            var subscription = new Subscription();
            subscription.UserID = userId;
            subscription.PodcastID = podcastId;
            subscription.SubscribedDate = DateTime.Now;

            database.Subscriptions.Add(subscription);
            await database.SaveChangesAsync();
            return Ok(subscription);
        }

        // Get user subscriptions
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSubscriptions(string userId)
        {
            var subscriptions = await database.Subscriptions
                .Where(s => s.UserID == userId)
                .ToListAsync();
            return Ok(subscriptions);
        }

        // Unsubscribe from podcast
        [HttpDelete]
        public async Task<IActionResult> Unsubscribe(string userId, int podcastId)
        {
            var subscription = await database.Subscriptions
                .FirstOrDefaultAsync(s => s.UserID == userId && s.PodcastID == podcastId);
            
            if (subscription != null)
            {
                database.Subscriptions.Remove(subscription);
                await database.SaveChangesAsync();
            }
            return Ok();
        }
    }
}