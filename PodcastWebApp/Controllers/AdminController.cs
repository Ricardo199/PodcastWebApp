/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using Amazon.S3;
using Amazon.S3.Model;

namespace PodcastWebApp.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext database;
        private readonly UserManager<User> userManager;
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName = "podcast-media-files-rb-2025";

        public AdminController(ApplicationDbContext db, UserManager<User> userMgr, IAmazonS3 s3)
        {
            database = db;
            userManager = userMgr;
            s3Client = s3;
        }

        // View to manage S3 files
        [HttpGet("s3-files")]
        public IActionResult S3Files()
        {
            return View();
        }

        // API endpoint to list S3 files
        [HttpGet("api/s3-files")]
        public async Task<IActionResult> GetS3Files()
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };

                var response = await s3Client.ListObjectsV2Async(request);
                
                var files = response.S3Objects
                    .Where(obj => obj.Key.EndsWith(".mp3") || obj.Key.EndsWith(".m4a") || obj.Key.EndsWith(".wav"))
                    .Select(obj => new
                    {
                        FileName = obj.Key,
                        Size = FormatFileSize((long)obj.Size),
                        SizeBytes = obj.Size,
                        LastModified = obj.LastModified,
                        Url = $"https://{bucketName}.s3.us-east-1.amazonaws.com/{obj.Key}"
                    })
                    .ToList();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Import a specific S3 file as an episode
        [HttpPost("api/import-s3-file")]
        public async Task<IActionResult> ImportS3File([FromBody] ImportRequest request)
        {
            try
            {
                // Get or create default podcast
                var podcast = await database.Podcasts.FirstOrDefaultAsync();
                if (podcast == null)
                {
                    var user = await database.Users.FirstOrDefaultAsync();
                    if (user == null)
                    {
                        return BadRequest("No users found. Create a user first.");
                    }

                    podcast = new Podcast
                    {
                        Title = "The Deep Dive",
                        Description = "Everything and anything in depth",
                        Category = "Education",
                        CoverImageURL = "https://via.placeholder.com/300",
                        CreatorID = user.Id,
                        CreatedDate = DateTime.UtcNow
                    };
                    database.Podcasts.Add(podcast);
                    await database.SaveChangesAsync();
                }

                // Check if episode already exists
                if (await database.Episodes.AnyAsync(e => e.AudioFileURL == request.Url))
                {
                    return BadRequest("This episode already exists in the database.");
                }

                // Create episode
                var episode = new Episode
                {
                    PodcastID = podcast.PodcastID,
                    Title = request.Title ?? ExtractTitleFromFileName(request.FileName),
                    Description = request.Description ?? "Imported from S3",
                    AudioFileURL = request.Url,
                    ThumbnailURL = "",
                    Duration = request.Duration ?? 0,
                    ReleaseDate = DateTime.UtcNow,
                    Host = request.Host ?? "Unknown",
                    Topic = request.Topic ?? "General",
                    Views = 0,
                    PlayCount = 0,
                    IsApproved = true
                };

                database.Episodes.Add(episode);
                await database.SaveChangesAsync();

                return Ok(new { message = "Episode imported successfully!", episodeId = episode.EpisodeID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Import all S3 files at once
        [HttpPost("api/import-all-s3")]
        public async Task<IActionResult> ImportAllS3Files()
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };

                var response = await s3Client.ListObjectsV2Async(request);
                var audioFiles = response.S3Objects
                    .Where(obj => obj.Key.EndsWith(".mp3") || obj.Key.EndsWith(".m4a") || obj.Key.EndsWith(".wav"))
                    .ToList();

                // Get or create default podcast
                var podcast = await database.Podcasts.FirstOrDefaultAsync();
                if (podcast == null)
                {
                    var user = await database.Users.FirstOrDefaultAsync();
                    if (user == null)
                    {
                        return BadRequest("No users found. Create a user first.");
                    }

                    podcast = new Podcast
                    {
                        Title = "The Deep Dive",
                        Description = "Everything and anything in depth",
                        Category = "Education",
                        CoverImageURL = "https://via.placeholder.com/300",
                        CreatorID = user.Id,
                        CreatedDate = DateTime.UtcNow
                    };
                    database.Podcasts.Add(podcast);
                    await database.SaveChangesAsync();
                }

                int importedCount = 0;
                foreach (var file in audioFiles)
                {
                    var url = $"https://{bucketName}.s3.us-east-1.amazonaws.com/{file.Key}";
                    
                    // Check if already exists
                    if (await database.Episodes.AnyAsync(e => e.AudioFileURL == url))
                    {
                        continue;
                    }

                    var episode = new Episode
                    {
                        PodcastID = podcast.PodcastID,
                        Title = ExtractTitleFromFileName(file.Key),
                        Description = "Imported from S3",
                        AudioFileURL = url,
                        ThumbnailURL = "",
                        Duration = 0,
                        ReleaseDate = (DateTime)file.LastModified,
                        Host = "Unknown",
                        Topic = "General",
                        Views = 0,
                        PlayCount = 0,
                        IsApproved = true
                    };

                    database.Episodes.Add(episode);
                    importedCount++;
                }

                await database.SaveChangesAsync();

                return Ok(new { message = $"Successfully imported {importedCount} episodes!", count = importedCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper methods
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string ExtractTitleFromFileName(string fileName)
        {
            // Remove path and extension
            var name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            // Replace underscores with spaces
            name = name.Replace("_", " ").Replace("-", " ");
            // Remove GUID-like patterns
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}", "").Trim();
            return name;
        }

        // Existing methods...
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await userManager.DeleteAsync(user);
            }
            return Ok();
        }

        [HttpGet("episodes/pending")]
        public async Task<IActionResult> GetPendingEpisodes()
        {
            var episodes = await database.Episodes.ToListAsync();
            return Ok(episodes);
        }

        [HttpPut("episodes/{episodeId}/approve")]
        public async Task<IActionResult> ApproveEpisode(int episodeId)
        {
            var episode = await database.Episodes.FindAsync(episodeId);
            if (episode == null)
            {
                return NotFound();
            }
            
            return Ok("Episode approved");
        }

        [HttpDelete("episodes/{episodeId}")]
        public async Task<IActionResult> DeleteEpisode(int episodeId)
        {
            var episode = await database.Episodes.FindAsync(episodeId);
            if (episode != null)
            {
                database.Episodes.Remove(episode);
                await database.SaveChangesAsync();
            }
            return Ok();
        }
    }

    public class ImportRequest
    {
        public string FileName { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Host { get; set; }
        public string? Topic { get; set; }
        public int? Duration { get; set; }
    }
}