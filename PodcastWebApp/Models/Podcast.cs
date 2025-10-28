/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using System.ComponentModel.DataAnnotations;

namespace PodcastWebApp.Models
{
    public class Podcast
    {
        [Key]
        public int PodcastID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string CreatorID { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string CoverImageURL { get; set; } = "";
        public string Category { get; set; } = "";
        public virtual User? Creator { get; set; }
        public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}