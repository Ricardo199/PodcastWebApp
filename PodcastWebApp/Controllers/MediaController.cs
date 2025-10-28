/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using TagLib;

namespace PodcastWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IAmazonS3 s3Client;
        private readonly ApplicationDbContext _context;
        private readonly string bucketName = "podcast-media-files-rb-2025";
        private readonly string awsRegion = "us-east-1";

        public MediaController(ApplicationDbContext context, IAmazonS3 s3Client)
        {
            _context = context;
            this.s3Client = s3Client;
        }

        // Upload file to S3
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string folder, int podcastId, string title, string description = "")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            var fileName = folder + "/" + Guid.NewGuid() + "_" + file.FileName;
            
            var request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.Key = fileName;
            request.InputStream = file.OpenReadStream();
            request.ContentType = file.ContentType;

            var response = await s3Client.PutObjectAsync(request);
            
            var fileUrl = $"https://{bucketName}.s3.{awsRegion}.amazonaws.com/{fileName}";

            int duration = 0;
            try
            {
                using var stream = file.OpenReadStream();
                var audioFile = TagLib.File.Create(new StreamFileAbstraction(file.FileName, stream, null));
                duration = (int)audioFile.Properties.Duration.TotalSeconds;
            }
            catch { }

            var episode = new Episode
            {
                Title = title,
                Description = description,
                PodcastID = podcastId,
                AudioFileURL = fileUrl,
                ReleaseDate = DateTime.Now,
                Duration = duration,
                Views = 0,
                PlayCount = 0,
                Host = "Unknown",
                Topic = "General",
                ThumbnailURL = "",
                IsApproved = true
            };

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();
            
            return Ok(new { Url = fileUrl, FileName = fileName, EpisodeId = episode.EpisodeID });
        }

        // Get file URL
        [HttpGet("url/{fileName}")]
        public IActionResult GetFileUrl(string fileName)
        {
            var fileUrl = "https://" + bucketName + ".s3.amazonaws.com/" + fileName;
            return Ok(new { Url = fileUrl });
        }

        // Delete file from S3
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var request = new DeleteObjectRequest();
            request.BucketName = bucketName;
            request.Key = fileName;

            await s3Client.DeleteObjectAsync(request);
            return Ok("File deleted");
        }

        // List all files
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var request = new ListObjectsV2Request();
            request.BucketName = bucketName;

            var response = await s3Client.ListObjectsV2Async(request);
            var files = response.S3Objects.Select(obj => new { 
                FileName = obj.Key, 
                Size = obj.Size, 
                LastModified = obj.LastModified,
                Url = "https://" + bucketName + ".s3.amazonaws.com/" + obj.Key
            });

            return Ok(files);
        }
    }
}