using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SneakerApp.Data;
using SneakerApp.Data.Migrations;
using SneakerApp.Models;
using System.Net.NetworkInformation;

namespace SneakerApp.Controllers
{
    public class ProductsController : Controller
    {
        // Pasul 10: Useri si roluri
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public double AverageRating(int productId)
        {
            var productRatings = db.Reviews
                       .Where(r => r.ProductId == productId && r.Score.HasValue)
                       .Select(r => r.Score.Value)
                       .ToList();

            double averageRating = productRatings.Any() ? productRatings.Average() : 0;

            return averageRating;
        }

        // INDEX (produse + categorie)
        [HttpGet]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category").Include("User").OrderBy(p => p.Name).ToList();
            var productAverages = new Dictionary<int, double?>();

            // Calculate average rating for each product
            foreach (var prod in products)
            {
                var avgScore = db.Reviews
                                 .Where(r => r.ProductId == prod.Id)
                                 .Select(r => r.Score)
                                 .ToList()
                                 .DefaultIfEmpty(null)
                                 .Average();

                productAverages[prod.Id] = avgScore;
            }

            ViewBag.Products = products;
            ViewBag.Averages = productAverages;

            var search = "";
            var sortBy = HttpContext.Request.Query["sortBy"].ToString();

            // Search Logic
            if (!string.IsNullOrEmpty(HttpContext.Request.Query["search"]))
            {
                search = HttpContext.Request.Query["search"].ToString().Trim();
                List<int> productIds = db.Products.Where(pr => pr.Name.Contains(search) || pr.Category.CategoryName.Contains(search))
                                                  .Select(p => p.Id).ToList();

                products = db.Products.Where(p => productIds.Contains(p.Id))
                                      .Include("Category")
                                      .OrderBy(p => p.Name)
                                      .ToList();
            }

            // Sorting Logic (one parameter for both price and rating)
            if (sortBy == "price_asc")
            {
                products = products.OrderBy(p => p.Price).ToList();
            }
            else if (sortBy == "price_desc")
            {
                products = products.OrderByDescending(p => p.Price).ToList();
            }
            else if (sortBy == "rating_asc")
            {
                products = products.OrderBy(p => productAverages.GetValueOrDefault(p.Id, 0)).ToList();
            }
            else if (sortBy == "rating_desc")
            {
                products = products.OrderByDescending(p => productAverages.GetValueOrDefault(p.Id, 0)).ToList();
            }

            ViewBag.SearchString = search;
            ViewBag.SortBy = sortBy;

            // Pagination Logic
            int _perPage = 6;
            int totalItems = products.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = (currentPage - 1) * _perPage;

            var paginatedProducts = products.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling(totalItems / (float)_perPage);
            ViewBag.Products = paginatedProducts;

            // Build Pagination Base URL
            ViewBag.PaginationBaseUrl = "/Products/Index/?page";

            if (!string.IsNullOrEmpty(search))
            {
                ViewBag.PaginationBaseUrl += "&search=" + search;
            }

            // Append sorting params to pagination URLs
            if (!string.IsNullOrEmpty(sortBy))
            {
                ViewBag.PaginationBaseUrl += "&sortPrice=" + sortBy;
            }
            

            return View();
        }





        // SHOW (afisare produs pe baza id-ului)
        [HttpGet]
        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category")
                                         .Include("Reviews")
                                         .Include("User")
                                         .Include("Reviews.User")
                              .Where(prod => prod.Id == id)
                              .First();

            SetAccessRights();

            return View(product);
        }

        // SHOW cu POST (adaugare review) - TBA
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Review review)
        {
            review.Date = DateTime.Now;

            if (review.Score == -1)
                review.Score = null;

            //preluam id-ul utilizatorului care posteaza comentariul
            review.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Reviews.Add(review);
                db.SaveChanges();
                return Redirect("/Products/Show/" + review.ProductId);
            }
            else
            {
                Product prod = db.Products.Include("Category")
                                          .Include("User")
                                          .Include("Reviews")
                                          .Include("Reviews.User")
                                          .Where(prod => prod.Id == review.ProductId)
                                          .First();

                //return Redirect("/Products/Show/" + comm.ProductId);

                SetAccessRights();

                return View(prod);
            }
        }

        // NEW
        [HttpGet]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> New(Product product, IFormFile Image)
        {
            // preluam id ul userului care posteza produsul
            product.UserId = _userManager.GetUserId(User);
            product.Verified = false;

            if (Image != null && Image.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                return View(product);
                }

                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images",
                Image.FileName);
                var databaseFileName = "/images/" + Image.FileName;
                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Image.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(product.Image));
                product.Image = databaseFileName;
            }

            if (TryValidateModel(product))
            {
                if (User.IsInRole("Admin"))
                {
                    product.Verified = true;
                }

                db.Products.Add(product);
                await db.SaveChangesAsync();

                if (User.IsInRole("Editor"))
                    TempData["message"] = "Produsul asteapta verificarea administratorului.";
                else
                    TempData["message"] = "Produsul a fost adaugat";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index", "Products");
            }
                product.Categ = GetAllCategories();
                return View(product);
            
        }

        // EDIT
        [HttpGet]
        // adminii editeaza orice produs, in timp ce editorul doar pe cel pe care l-a adaugat
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(prod => prod.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            if ((_userManager.GetUserId(User) == product.UserId) || User.IsInRole("Admin"))
            {
                return View(product);
            }
            else
            {

                TempData["message"] = "Nu aveti dreptul sa modificati un produs al altui brand!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }


        }

        // se verifica rolul utilizatorilor care au dreptul sa editeze
        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int id, Product requestProduct, IFormFile Image)
        {
            // Retrieve the product to edit
            Product product = db.Products.Find(id);

            // If the product is not found, return NotFound
            if (product == null)
            {
                return NotFound();
            }

            // Check if the user is allowed to edit the product
            if ((_userManager.GetUserId(User) == product.UserId) || User.IsInRole("Admin"))
            {
                // Assign the values from the requestProduct to the product entity
                product.Name = requestProduct.Name;
                product.Description = requestProduct.Description;
                product.Price = requestProduct.Price;
                product.Stock = requestProduct.Stock;
                product.CategoryId = requestProduct.CategoryId;
                product.Verified = requestProduct.Verified;

                if (Image != null && Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                    var fileExtension = Path.GetExtension(Image.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProductImage", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov).");
                        requestProduct.Categ = GetAllCategories();
                        return View(requestProduct);
                    }


                    var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                    var databaseFileName = "/images/" + Image.FileName;

                    // Save the new image
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    product.Image = databaseFileName;
                }

                db.SaveChanges();

                if (User.IsInRole("Editor"))
                    TempData["message"] = "Produsul a fost modificat! Se asteapta verificarea.";
                else
                    TempData["message"] = "Produsul a fost modificat";

                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                // If the user does not have permission to edit this product
                requestProduct.Categ = GetAllCategories();
                TempData["message"] = "Nu aveti dreptul sa modificati un produs al altui brand!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        // DELETE

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);

            if ((_userManager.GetUserId(User) == product.UserId) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs al altui brand!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // conditii de afisare pt butoanele de edit si delete aflate in view uri

        private void SetAccessRights()
        {
            ViewBag.Buttons = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.Buttons = true;
            }

            ViewBag.CurrentUser = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Verify()
        {
            // Retrieve only unverified products
            var products = db.Products.Include(p => p.Category)
                               .ToList()
                               .Where(p => !p.Verified)
                               .ToList();

            // Pass the products to the view
            return View(products);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Verify(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Verified = true;
            db.SaveChanges();

            TempData["message"] = "Produsul a fost verificat";
            TempData["messageType"] = "alert-success";

            return RedirectToAction("Index");
        }




        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

    }
}
