using CinemaSystem.Models;
using CinemaSystem.Repositories;
using CinemaSystem.ViewModels.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace CinemaSystem.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepo<Movie> _movieRepo;
        private readonly IRepo<Cart> _cartRepo;

        public CartController(UserManager<ApplicationUser> userManager, IRepo<Movie> movieRepo, IRepo<Cart> cartRepo)
        {
            _userManager = userManager;
            _movieRepo = movieRepo;
            _cartRepo = cartRepo;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var cartItems = _cartRepo.GetAll(c => c.ApplicationUserId == user.Id, Includes: [c => c.Movie]).ToList();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int movieId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var movie = await _movieRepo.GetOneAsync(m => m.Id == movieId);
            if (movie == null) return NotFound();
            var existingCartItem = await _cartRepo.GetOneAsync(c => c.ApplicationUserId == user.Id && c.MovieId == movieId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                _cartRepo.Update(existingCartItem);
                await _cartRepo.CommitAsync();
            }
            else
            {

                var CartItem = new Cart
                {
                    ApplicationUserId = user.Id,
                    MovieId = movieId,
                    Quantity = quantity
                };
                await _cartRepo.AddAsync(CartItem);
                await _cartRepo.CommitAsync();
            }
            TempData["SuccessMessage"] = "Movie added to cart successfully!";
            return RedirectToAction("ShowMovieDetails", "Home", new { id = movieId });
        }
        public async Task<IActionResult> Remove(int MovieId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var cartItem = await _cartRepo.GetOneAsync(c=>c.ApplicationUserId==userId&&c.MovieId==MovieId);
            if (cartItem != null)
            {
                _cartRepo.Remove(cartItem);
                await _cartRepo.CommitAsync();
                TempData["SuccessMessage"] = "Movie removed from cart successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Movie not found in cart.";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DecreaseQuantity(int MovieId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var cartItem = await _cartRepo.GetOneAsync(c => c.ApplicationUserId == userId && c.MovieId == MovieId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    _cartRepo.Update(cartItem);
                }
                else if (cartItem.Quantity == 1)
                {
                    _cartRepo.Remove(cartItem);
                    TempData["SuccessMessage"] = "Movie removed from cart successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Movie not found in cart.";
                }
                await _cartRepo.CommitAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Movie not found in cart.";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> IncreaseQuantity(int MovieId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var cartItem = await _cartRepo.GetOneAsync(c => c.ApplicationUserId == userId && c.MovieId == MovieId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                _cartRepo.Update(cartItem);
                await _cartRepo.CommitAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Movie not found in cart.";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Pay()
        {
            var options = new SessionCreateOptions
            {
                AllowedPaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Cancel",
            };
            var user = await _userManager.GetUserAsync(User);
            if(user==null) return NotFound();
            var cartItems = _cartRepo.GetAll(c => c.ApplicationUserId == user.Id, Includes: [c => c.Movie]).ToList();
            foreach (var item in cartItems)
            {
                var lineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Movie.Price * 100),
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description
                        },
                    },
                    Quantity = item.Quantity,
                };
                options.LineItems.Add(lineItem);
            }
            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }
    }
}
