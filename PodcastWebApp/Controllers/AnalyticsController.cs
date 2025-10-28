/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;

namespace PodcastWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext database;

        public AnalyticsController(ApplicationDbContext db)
        {
            database = db;
        }

        // Get top 10 episodes by views
        [HttpGet("top-episodes")]
        public async Task<IActionResult> GetTopEpisodes()
        {
            var topEpisodes = await database.Episodes
                .OrderByDescending(e => e.Views)
                .Take(10)
                .ToListAsync();
            return Ok(topEpisodes);
        }

        // Get episode statistics
        [HttpGet("episode-stats/{episodeId}")]
        public async Task<IActionResult> GetEpisodeStats(int episodeId)
        {
            var episode = await database.Episodes.FindAsync(episodeId);
            if (episode == null)
            {
                return NotFound();
            }

            var stats = new
            {
                EpisodeId = episode.EpisodeID,
                Title = episode.Title,
                Views = episode.Views,
                PlayCount = episode.PlayCount,
                Duration = episode.Duration
            };
            return Ok(stats);
        }

        // Get total statistics
        [HttpGet("total-stats")]
        public async Task<IActionResult> GetTotalStats()
        {
            var totalEpisodes = await database.Episodes.CountAsync();
            var totalViews = await database.Episodes.SumAsync(e => e.Views);
            var totalPodcasts = await database.Podcasts.CountAsync();

            var stats = new
            {
                TotalEpisodes = totalEpisodes,
                TotalViews = totalViews,
                TotalPodcasts = totalPodcasts
            };
            return Ok(stats);
        }
    }
}