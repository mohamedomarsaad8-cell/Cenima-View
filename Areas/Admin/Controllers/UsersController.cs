using CinemaSystem.Models;
using CinemaSystem.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE}")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index(int page = 1)
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if(user.LockoutEnd!=null&&user.LockoutEnd>DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(2);
            }
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
