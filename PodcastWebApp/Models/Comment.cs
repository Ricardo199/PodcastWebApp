/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using System.ComponentModel.DataAnnotations.Schema;

namespace PodcastWebApp.Models
{
    public class Comment
    {
        public string Id { get; set; } = "";
        public int EpisodeId { get; set; }

        [ForeignKey("UserID")]
        public string UserId { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}