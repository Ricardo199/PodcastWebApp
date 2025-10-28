/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using System.ComponentModel.DataAnnotations;

namespace PodcastWebApp.Models
{
    public enum UserRole { Podcaster, Listener, Admin }

    public class UserAccount
    {
        [Key]
        public Guid UserID { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public UserRole Role { get; set; }
    }
}