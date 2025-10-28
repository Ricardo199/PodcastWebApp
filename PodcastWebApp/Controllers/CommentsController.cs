/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;

namespace PodcastWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private static List<dynamic> commentsList = new List<dynamic>();

        // Add comment to episode
        [HttpPost]
        public IActionResult AddComment(int episodeId, string userId, string text)
        {
            var newComment = new { Id = Guid.NewGuid().ToString(), EpisodeId = episodeId, UserId = userId, Text = text, CreatedAt = DateTime.Now };
            commentsList.Add(newComment);
            return Ok(newComment);
        }

        // Get comments for episode
        [HttpGet("episode/{episodeId}")]
        public IActionResult GetCommentsForEpisode(int episodeId)
        {
            var episodeComments = new List<dynamic>();
            foreach (var comment in commentsList)
            {
                if (comment.EpisodeId == episodeId)
                {
                    episodeComments.Add(comment);
                }
            }
            return Ok(episodeComments);
        }

        // Update comment within 24 hours
        [HttpPut("{id}")]
        public IActionResult ModifyComment(string id, string userId, string text)
        {
            for (int i = 0; i < commentsList.Count; i++)
            {
                var comment = commentsList[i];
                if (comment.Id == id)
                {
                    if (comment.UserId != userId)
                    {
                        return Forbid();
                    }
                    
                    var hoursSinceCreated = (DateTime.Now - comment.CreatedAt).TotalHours;
                    if (hoursSinceCreated > 24)
                    {
                        return BadRequest("Cannot modify comment after 24 hours");
                    }
                    
                    var updated = new { Id = comment.Id, EpisodeId = comment.EpisodeId, UserId = comment.UserId, Text = text, CreatedAt = comment.CreatedAt };
                    commentsList[i] = updated;
                    return Ok(updated);
                }
            }
            return NotFound();
        }
    }
}