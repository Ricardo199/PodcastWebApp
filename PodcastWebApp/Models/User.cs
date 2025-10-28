/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Identity;

namespace PodcastWebApp.Models
{
    // User class using ASP.NET Identity
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string AvatarURL { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Podcast> Podcasts { get; set; } = new List<Podcast>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}