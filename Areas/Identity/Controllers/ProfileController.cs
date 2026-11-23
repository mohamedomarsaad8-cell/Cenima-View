using CinemaSystem.Models;
using CinemaSystem.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]

    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> UserDetails()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user==null) return NotFound();
            ApplicationUserDetailsVM applicationUserDetailsVM = new ApplicationUserDetailsVM
            {
                FullName = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return View(applicationUserDetailsVM);
        }
        public async Task<IActionResult> UpdateProfile(ApplicationUserDetailsVM applicationUserDetailsVM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            user.Name = applicationUserDetailsVM.FullName;
            user.Email = applicationUserDetailsVM.Email;
            user.PhoneNumber = applicationUserDetailsVM.PhoneNumber;
            user.Address = applicationUserDetailsVM.Address;


            var result = await _userManager.UpdateAsync(user);
            var Error = "";
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    Error += string.Join(',', error.Description);
                }
                TempData["Error"] = Error;
            }
            else 
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            return RedirectToAction(nameof(UserDetails));
        }
        public async Task<IActionResult> UpdatePassword(ApplicationUserDetailsVM applicationUserDetailsVM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var result = await _userManager.ChangePasswordAsync(user, applicationUserDetailsVM.CurrentPassword, applicationUserDetailsVM.NewPassword);
            var Error = "";
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Error += string.Join(',', error.Description);
                }
                TempData["Error"] = Error;
            }
            else
            {
                TempData["SuccessMessage"] = "Password updated successfully!";
            }
            return RedirectToAction(nameof(UserDetails));
        }
    }
}
