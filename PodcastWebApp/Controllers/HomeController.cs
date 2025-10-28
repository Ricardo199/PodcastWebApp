using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;
using PodcastWebApp.Models.ViewModels;
using System.Diagnostics;

namespace PodcastWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new HomeViewModel
                {
                    FeaturedEpisodes = await _context.Episodes
                        .Include(e => e.Podcast)
                        .OrderByDescending(e => e.Views)
                        .Take(6)
                        .ToListAsync(),
                    
                    LatestEpisodes = await _context.Episodes
                        .Include(e => e.Podcast)
                        .OrderByDescending(e => e.ReleaseDate)
                        .Take(8)
                        .ToListAsync(),
                    
                    FeaturedPodcasts = await _context.Podcasts
                        .Include(p => p.Creator)
                        .Include(p => p.Episodes)
                        .OrderByDescending(p => p.Episodes.Count)
                        .Take(4)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");
                
                // Return empty model to prevent null reference
                return View(new HomeViewModel
                {
                    FeaturedEpisodes = new List<Models.Episode>(),
                    LatestEpisodes = new List<Models.Episode>(),
                    FeaturedPodcasts = new List<Models.Podcast>()
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}