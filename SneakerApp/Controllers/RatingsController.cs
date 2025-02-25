using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    public class RatingsController : Controller
    {
        // Pasul 10: Useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RatingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Editarea unui rating asociat unui produs
        // Se poate edita un rating doar de catre utilizatorul
        // care a postat rating-ul respectiv

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Rating rat = db.Ratings.Find(id);

            if (rat.UserId == _userManager.GetUserId(User))
            {
                return View(rat);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati rating-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Rating requestRating)
        {
            Rating rat = db.Ratings.Find(id);

            if (rat.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {

                    rat.Score = requestRating.Score;

                    db.SaveChanges();

                    return Redirect("/Products/Show/" + rat.ProductId);
                }
                else
                {
                    return View(requestRating);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati rating-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }


        // DELETE
        // Se poate sterge rating-ul doar de catre utilizatorii 
        // care au postat rating-ul respectiv
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Rating rat = db.Ratings.Find(id);

            if (rat.UserId == _userManager.GetUserId(User))
            {
                db.Ratings.Remove(rat);
                db.SaveChanges();
                return Redirect("/Products/Show/" + rat.ProductId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati rating-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }
    }
}
