/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using System.ComponentModel.DataAnnotations;

namespace PodcastWebApp.Models
{
    public class Subscription
    {
        [Key]
        public int SubscriptionID { get; set; }
        public string UserID { get; set; } = null!;
        public int PodcastID { get; set; }
        public DateTime SubscribedDate { get; set; }
    }
}