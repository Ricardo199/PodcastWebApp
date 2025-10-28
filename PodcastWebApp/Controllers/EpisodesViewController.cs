using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using PodcastWebApp.Models.ViewModels;

namespace PodcastWebApp.Controllers
{
    public class EpisodesViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EpisodesViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var episodes = await _context.Episodes.Include(e => e.Podcast).ToListAsync();
            return View("~/Views/Episodes/Index.cshtml", episodes);
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

            return View("~/Views/Episodes/Details.cshtml", model);
        }
    }
}
