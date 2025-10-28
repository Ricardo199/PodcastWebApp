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
    [Route("Podcasts")]
    public class PodcastsViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PodcastsViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Podcasts
        [HttpGet("")]
        [HttpGet("Index")]
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
            return View("~/Views/Podcasts/Index.cshtml", podcasts);
        }

        // GET: /Podcasts/Details/5
        [HttpGet("Details/{id}")]
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

            return View("~/Views/Podcasts/Details.cshtml", podcast);
        }

        // GET: /Podcasts/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Podcasts/Create.cshtml");
        }

        // POST: /Podcasts/Create
        [HttpPost("Create")]
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
            return View("~/Views/Podcasts/Create.cshtml", podcast);
        }

        // GET: /Podcasts/S3Files
        [HttpGet("S3Files")]
        public IActionResult S3Files()
        {
            return View("~/Views/Podcasts/S3Files.cshtml");
        }
    }
}