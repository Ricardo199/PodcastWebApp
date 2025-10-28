/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using System.ComponentModel.DataAnnotations;

namespace PodcastWebApp.Models
{
    public class Episode
    {
        [Key]
        public int EpisodeID { get; set; }
        public int PodcastID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public int Duration { get; set; } = 0;
        public int PlayCount { get; set; } = 0;
        public string AudioFileURL { get; set; } = "";
        public int Views { get; set; } = 0;
        public string Host { get; set; } = "";
        public string Topic { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public bool IsApproved { get; set; } = true;
        public virtual Podcast? Podcast { get; set; }
    }
}