using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastWebApp.Models;
using PodcastWebApp.Models.ViewModels;
using PodcastWebApp.Data;

namespace PodcastWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string firstName, string lastName, string role)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                AvatarURL = "https://via.placeholder.com/150",
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Dashboard");
            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            if (roles.Contains("Podcaster"))
                return RedirectToAction("PodcasterDashboard");
            else if (roles.Contains("Admin"))
                return RedirectToAction("AdminDashboard");
            else
                return RedirectToAction("ListenerDashboard");
        }

        [Authorize(Roles = "Podcaster")]
        public async Task<IActionResult> PodcasterDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            
            // Fetch user's podcasts with episodes
            var podcasts = await _context.Podcasts
                .Where(p => p.CreatorID == user.Id)
                .Include(p => p.Episodes)
                .ToListAsync();
            
            // Calculate stats
            var totalViews = podcasts.SelectMany(p => p.Episodes).Sum(e => e.Views);
            var totalEpisodes = podcasts.SelectMany(p => p.Episodes).Count();
            var totalComments = 0; // Update when you have comments table
            var totalSubscribers = 0; // Update when you have subscriptions table
            
            // Create view model
            var viewModel = new PodcasterDashboardViewModel
            {
                TotalViews = totalViews,
                TotalEpisodes = totalEpisodes,
                TotalComments = totalComments,
                TotalSubscribers = totalSubscribers,
                Podcasts = podcasts
            };
            
            return View(viewModel);
        }

        [Authorize(Roles = "Listener")]
        public IActionResult ListenerDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string bio)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FirstName = firstName ?? "";
            user.LastName = lastName ?? "";
            
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }
            
            return RedirectToAction("Profile");
        }

        [Authorize]
        public IActionResult Settings()
        {
            return View();
        }
    }
}