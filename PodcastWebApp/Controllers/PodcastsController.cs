/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;

namespace PodcastWebApp.Controllers
{
    public class PodcastsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PodcastsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Podcasts
        public async Task<IActionResult> Index(string search, string category)
        {
            var podcastsQuery = _context.Podcasts
                .Include(p => p.Creator)
                .Include(p => p.Episodes)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                podcastsQuery = podcastsQuery.Where(p => 
                    p.Title.Contains(search) || 
                    p.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                podcastsQuery = podcastsQuery.Where(p => p.Category == category);
                ViewBag.Category = category;
            }

            var podcasts = await podcastsQuery.ToListAsync();
            return View(podcasts);
        }

        // GET: /Podcasts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var podcast = await _context.Podcasts
                .Include(p => p.Creator)
                .Include(p => p.Episodes)
                .FirstOrDefaultAsync(p => p.PodcastID == id);

            if (podcast == null)
            {
                return NotFound();
            }

            return View(podcast);
        }

        // GET: /Podcasts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Podcasts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Category,CoverImageURL,CreatorID")] Models.Podcast podcast)
        {
            if (ModelState.IsValid)
            {
                podcast.CreatedDate = DateTime.UtcNow;
                _context.Podcasts.Add(podcast);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(podcast);
        }

        // GET: /Podcasts/S3Files
        public IActionResult S3Files()
        {
            return View();
        }

        // API: GET /api/podcasts/user
        [HttpGet]
        [Route("api/podcasts/user")]
        public async Task<IActionResult> GetUserPodcasts()
        {
            // Get all podcasts for now (you can filter by current user later)
            var podcasts = await _context.Podcasts
                .Select(p => new 
                { 
                    podcastID = p.PodcastID, 
                    title = p.Title 
                })
                .ToListAsync();

            return Json(podcasts);
        }
    }
}