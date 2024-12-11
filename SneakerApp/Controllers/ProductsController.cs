using SneakerApp.Data;
using SneakerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SneakerApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;
        public ProductsController(ApplicationDbContext context)
        {
            db = context;
        }

        // INDEX (produse + categorie)
        [HttpGet]
        public IActionResult Index()
        {
            var products = db.Products.Include("Category");
            ViewBag.Products = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        // SHOW (afisare produs pe baza id-ului)
        [HttpGet]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category").Include("Reviews")
                              .Where(prod => prod.Id == id)
                              .First();

            return View(product);
        }

        // SHOW cu POST (adaugare review) - TBA

        [HttpGet]
        public IActionResult New()
        {
            Product product = new Product();

            product.Categ = GetAllCategories();

            return View(product);
        }

        [HttpPost]
        public IActionResult New(Product product)
        {
            product.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat";
                return RedirectToAction("Index");
            }
            else
            {
                return View(product);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            product.Categ = GetAllCategories();

            return View(product);

        }

        [HttpPost]
        public IActionResult Edit(int id, Product requestProduct)
        {
            Product product = db.Products.Find(id);

            if (ModelState.IsValid)
            {
                product.Name = requestProduct.Name;
                product.Description = requestProduct.Description;
                product.Price = requestProduct.Price;
                product.Stock = requestProduct.Stock;
                product.Rating = requestProduct.Rating;
                product.CategoryId = requestProduct.CategoryId;
                TempData["message"] = "Produsul a fost modificat";
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            else
            {
                requestProduct.Categ = GetAllCategories();
                return View(requestProduct);
            }
        }

        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["message"] = "Articolul a fost sters";
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
