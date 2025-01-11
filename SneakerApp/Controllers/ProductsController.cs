 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    public class ProductsController : Controller
    {
        // Pasul 10: Useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProductsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
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
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category").Include("User").ToList();

            // Dictionary to store product Id and average score
            var productAverages = new Dictionary<int, double?>();

            foreach (var prod in products)
            {
                // Force client-side evaluation using ToList
                var avgScore = db.Reviews
                                 .Where(r => r.ProductId == prod.Id)
                                 .Select(r => r.Score)
                                 .ToList() // Executes the query
                                 .DefaultIfEmpty(null)
                                 .Average();

                productAverages[prod.Id] = avgScore; // Store the average score
            }

            ViewBag.Products = products;
            ViewBag.Averages = productAverages; // Pass averages to the view

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }



        // SHOW (afisare produs pe baza id-ului)
        [HttpGet]
        [Authorize(Roles = "User,Editor,Admin")]
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
        public IActionResult New(Product product)
        {


            // preluam id ul userului care posteza produsul
            product.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                product.Categ = GetAllCategories();
                return View(product);
            }
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
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);

            if (ModelState.IsValid)
            {
                if ((_userManager.GetUserId(User) == product.UserId) || User.IsInRole("Admin"))
                {
                    product.Name = requestProduct.Name;
                    product.Description = requestProduct.Description;
                    product.Price = requestProduct.Price;
                    product.Stock = requestProduct.Stock;
                    //product.Rating = requestProduct.Rating;
                    product.CategoryId = requestProduct.CategoryId;
                    TempData["message"] = "Produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    product.Categ = GetAllCategories();
                    TempData["message"] = "Nu aveti dreptul sa modificati un produs al altui brand!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }


            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
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
