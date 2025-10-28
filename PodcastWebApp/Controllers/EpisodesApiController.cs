using Microsoft.AspNetCore.Mvc;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using Amazon.S3;
using Amazon.S3.Model;

namespace PodcastWebApp.Controllers
{
    [Route("api/episodes")]
    [ApiController]
    public class EpisodesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<EpisodesApiController> _logger;
        private readonly string _bucketName = "podcast-media-files-rb-2025";
        private readonly string _awsRegion = "us-east-1";

        public EpisodesApiController(ApplicationDbContext context, IAmazonS3 s3Client, ILogger<EpisodesApiController> logger)
        {
            _context = context;
            _s3Client = s3Client;
            _logger = logger;
        }

        // POST: api/episodes
        [HttpPost]
        public async Task<IActionResult> CreateEpisode()
        {
            try
            {
                var form = await Request.ReadFormAsync();

                // Get the uploaded audio file
                var audioFile = form.Files.GetFile("audioFile");
                if (audioFile == null || audioFile.Length == 0)
                {
                    return BadRequest(new { message = "No audio file provided" });
                }

                // Validate file type
                var allowedTypes = new[] { "audio/mpeg", "audio/wav", "audio/mp4", "audio/x-m4a", "audio/mp3" };
                var fileName = audioFile.FileName.ToLower();
                if (!allowedTypes.Contains(audioFile.ContentType.ToLower()) && 
                    !fileName.EndsWith(".mp3") && 
                    !fileName.EndsWith(".wav") && 
                    !fileName.EndsWith(".m4a"))
                {
                    return BadRequest(new { message = "Invalid file type. Only MP3, WAV, and M4A files are allowed." });
                }

                // Validate file size (500MB max)
                if (audioFile.Length > 500 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size must be less than 500MB" });
                }

                // Generate unique filename for S3
                var fileExtension = Path.GetExtension(audioFile.FileName);
                var s3FileName = $"episodes/{Guid.NewGuid()}{fileExtension}";

                _logger.LogInformation($"Attempting to upload file to S3: Bucket={_bucketName}, Key={s3FileName}");

                // Upload to S3
                try
                {
                    using (var stream = audioFile.OpenReadStream())
                    {
                        var uploadRequest = new PutObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = s3FileName,
                            InputStream = stream,
                            ContentType = audioFile.ContentType
                            // REMOVED: CannedACL - bucket uses bucket policy instead of ACLs
                        };

                        var response = await _s3Client.PutObjectAsync(uploadRequest);
                        
                        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogError($"S3 upload failed with status code: {response.HttpStatusCode}");
                            return StatusCode(500, new { message = $"Failed to upload file to S3. Status: {response.HttpStatusCode}" });
                        }
                        
                        _logger.LogInformation($"Successfully uploaded file to S3: {s3FileName}");
                    }
                }
                catch (Amazon.S3.AmazonS3Exception s3Ex)
                {
                    _logger.LogError($"S3 Exception: {s3Ex.Message}, ErrorCode: {s3Ex.ErrorCode}, StatusCode: {s3Ex.StatusCode}");
                    return BadRequest(new 
                    { 
                        message = "AWS S3 Error", 
                        error = s3Ex.Message,
                        errorCode = s3Ex.ErrorCode,
                        statusCode = s3Ex.StatusCode.ToString(),
                        details = "Check your AWS credentials and S3 bucket permissions"
                    });
                }

                // Generate S3 URL
                var audioFileURL = $"https://{_bucketName}.s3.{_awsRegion}.amazonaws.com/{s3FileName}";

                // Extract audio duration using TagLib
                int duration = 0;
                try
                {
                    using var metadataStream = audioFile.OpenReadStream();
                    var audioFileTag = TagLib.File.Create(new StreamFileAbstraction(audioFile.FileName, metadataStream));
                    duration = (int)audioFileTag.Properties.Duration.TotalSeconds;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not extract audio duration: {ex.Message}");
                    // Use the duration from the form if TagLib fails
                    if (!string.IsNullOrEmpty(form["duration"]))
                    {
                        duration = int.Parse(form["duration"].ToString());
                    }
                }

                // Handle thumbnail upload (optional)
                string thumbnailURL = string.Empty;
                var thumbnailFile = form.Files.GetFile("thumbnailFile");
                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    var thumbExtension = Path.GetExtension(thumbnailFile.FileName);
                    var thumbFileName = $"thumbnails/{Guid.NewGuid()}{thumbExtension}";

                    try
                    {
                        using (var thumbStream = thumbnailFile.OpenReadStream())
                        {
                            var thumbUploadRequest = new PutObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = thumbFileName,
                                InputStream = thumbStream,
                                ContentType = thumbnailFile.ContentType
                            };

                            await _s3Client.PutObjectAsync(thumbUploadRequest);
                        }
                        
                        thumbnailURL = $"https://{_bucketName}.s3.{_awsRegion}.amazonaws.com/{thumbFileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to upload thumbnail: {ex.Message}");
                    }
                }

                // Create episode in database
                var episode = new Episode
                {
                    Title = form["title"].ToString(),
                    Description = form["description"].ToString(),
                    PodcastID = int.Parse(form["podcastId"].ToString()),
                    Host = form["host"].ToString() ?? string.Empty,
                    Topic = form["topic"].ToString() ?? string.Empty,
                    Duration = duration,
                    ReleaseDate = string.IsNullOrEmpty(form["releaseDate"]) 
                        ? DateTime.UtcNow 
                        : DateTime.Parse(form["releaseDate"].ToString()),
                    AudioFileURL = audioFileURL,
                    ThumbnailURL = thumbnailURL,
                    Views = 0,
                    PlayCount = 0,
                    IsApproved = true
                };

                _context.Episodes.Add(episode);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Episode uploaded successfully to S3", 
                    episodeId = episode.EpisodeID,
                    audioUrl = audioFileURL,
                    thumbnailUrl = thumbnailURL,
                    duration = episode.Duration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
                return BadRequest(new { message = "Failed to create episode", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // DELETE: api/episodes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEpisode(int id)
        {
            try
            {
                var episode = await _context.Episodes.FindAsync(id);
                if (episode == null) return NotFound(new { message = "Episode not found" });

                // Delete audio file from S3 if it exists
                if (!string.IsNullOrEmpty(episode.AudioFileURL) && episode.AudioFileURL.Contains(_bucketName))
                {
                    try
                    {
                        var uri = new Uri(episode.AudioFileURL);
                        var key = uri.AbsolutePath.TrimStart('/');
                        
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = key
                        };
                        
                        await _s3Client.DeleteObjectAsync(deleteRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not delete audio file from S3: {ex.Message}");
                    }
                }

                // Delete thumbnail from S3 if it exists
                if (!string.IsNullOrEmpty(episode.ThumbnailURL) && episode.ThumbnailURL.Contains(_bucketName))
                {
                    try
                    {
                        var uri = new Uri(episode.ThumbnailURL);
                        var key = uri.AbsolutePath.TrimStart('/');
                        
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = key
                        };
                        
                        await _s3Client.DeleteObjectAsync(deleteRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not delete thumbnail from S3: {ex.Message}");
                    }
                }

                // Delete from database
                _context.Episodes.Remove(episode);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Episode and files deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete episode: {ex.Message}");
                return BadRequest(new { message = "Failed to delete episode", error = ex.Message });
            }
        }

        [HttpPut("{id}/view")]
        public async Task<IActionResult> UpdateViewCount(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();
            
            episode.Views++;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
