using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        // Pasul 10: Useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // INDEX
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            return View();
        }

        // SHOW
        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        // NEW (view cu formularul pentru produs)
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
        }

            else
            {
                return View(cat);
            }
        }

        // EDIT
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Include("Products")
                                 .Include("Products.Reviews")
                                 .Where(c => c.Id == id)
                                 .First();
            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Include("Products")
                                             .Include("Products.Reviews")
                                             .Where(c => c.Id == id)
                                             .First();
            //if (ModelState.IsValid)
            //{

                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();
                Console.WriteLine("Yupiii");
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            //}
        
            //return View(requestCategory);
        }

        // DELETE

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            TempData["message"] = "Categoria a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
