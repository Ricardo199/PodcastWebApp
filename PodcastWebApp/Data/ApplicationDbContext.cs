/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Models;

namespace PodcastWebApp.Data
{
    // Database context class using ASP.NET Identity
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        // Constructor that receives database configuration options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        // Database tables (DbSets) that correspond to our models
        public DbSet<Podcast> Podcasts { get; set; }  // Podcasts table for podcast data
        public DbSet<Episode> Episodes { get; set; }  // Episodes table for episode data
        public DbSet<Subscription> Subscriptions { get; set; }  // Subscriptions table for user subscriptions

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                new IdentityRole { Id = "2", Name = "Podcaster", NormalizedName = "PODCASTER", ConcurrencyStamp = "2" },
                new IdentityRole { Id = "3", Name = "Listener", NormalizedName = "LISTENER", ConcurrencyStamp = "3" }
            );

            // Configure Subscription relationships
            modelBuilder.Entity<Subscription>()
                .HasOne<User>()
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne<Podcast>()
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PodcastID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}