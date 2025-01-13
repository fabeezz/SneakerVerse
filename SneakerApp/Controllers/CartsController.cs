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

            // Calculate total cart value
            double totalCartValue = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // Pass the cart items and total value to the view
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

            // Check if the item already exists in the cart
            var cartItem = db.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId && ci.UserId == userId);

            if (cartItem != null)
            {
                // Update quantity if already in cart
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

            // No stock update here
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
                    product.Stock -= cartItem.Quantity; // Decrement stock here
                }
            }

            // Logic to place the order (e.g., save to database)
            // Clear the cart after placing the order
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}