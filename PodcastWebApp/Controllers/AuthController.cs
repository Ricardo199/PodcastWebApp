/*
 * Authors: Ricardo Burgos & [Austin Chima]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PodcastWebApp.Data;
using PodcastWebApp.Models;

namespace PodcastWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AuthController(UserManager<User> userMgr, SignInManager<User> signInMgr)
        {
            userManager = userMgr;
            signInManager = signInMgr;
        }

        // Register new user
        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            var newUser = new User();
            newUser.UserName = username;
            newUser.Email = email;
            
            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                return Ok(newUser);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        // Login user
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
                if (result.Succeeded)
                {
                    return Ok(user);
                }
            }
            return Unauthorized();
        }
    }
}