using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using PodcastWebApp.Models.ViewModels;

namespace PodcastWebApp.Controllers
{
    public class EpisodesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EpisodesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var episodes = await _context.Episodes.Include(e => e.Podcast).ToListAsync();
            return View(episodes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var episode = await _context.Episodes.Include(e => e.Podcast).FirstOrDefaultAsync(e => e.EpisodeID == id);
            if (episode == null) return NotFound();

            var model = new EpisodeDetailViewModel
            {
                Episode = episode,
                Podcast = episode.Podcast!,
                Comments = new List<Comment>(), 
                IsSubscribed = false,
                CanEdit = false
            };

            return View(model);
        }

        // GET: Episodes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Episodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EpisodeID,Title,Description,PublishDate,FilePath,PodcastID")] Episode episode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(episode);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(episode);
        }
    }
}