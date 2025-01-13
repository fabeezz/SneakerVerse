using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    [Authorize(Roles = "User")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = db.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .ToList();

            double totalCartValue = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            ViewBag.TotalCartValue = totalCartValue;

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var product = db.Products.Find(productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (product.Stock < quantity)
            {
                return BadRequest("Insufficient stock.");
            }

            var cartItem = db.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId && ci.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                // Add new cart item
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = db.CartItems.Find(cartItemId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            // Restore stock to the product
            var product = db.Products.Find(cartItem.ProductId);
            if (product != null)
            {
                product.Stock += cartItem.Quantity;
            }

            db.CartItems.Remove(cartItem);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = db.CartItems.Where(ci => ci.UserId == userId).ToList();

            if (!cartItems.Any())
            {
                return BadRequest("Cart is empty.");
            }

            // Decrement stock for each product in the cart
            foreach (var cartItem in cartItems)
            {
                var product = db.Products.Find(cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                }
            }

            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}