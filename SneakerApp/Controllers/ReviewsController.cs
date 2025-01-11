using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    public class ReviewsController : Controller
    {
        // Pasul 10: Useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Editarea unui review asociat unui produs
        // Se poate edita un review doar de catre utilizatorul
        // care a postat review-ul respectiv

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                ViewBag.miau = rev.UserId == _userManager.GetUserId(User);
                return View(rev);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }


        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.Reviews.Find(id);


            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {

                    rev.Content = requestReview.Content;
                    if(requestReview.Score != null)
                        rev.Score = requestReview.Score;

                    db.SaveChanges();

                    return Redirect("/Products/Show/" + rev.ProductId);
                }
                else
                {
                    return View(requestReview);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

        // DELETE
        // Se poate sterge review-ul doar de catre userii cu rolul de Admin
        // sau de catre utilizatorii cu rolul de User sau Editor (doar daca
        // acel review a fost postat de catre acestia
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);

            if (rev.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Reviews.Remove(rev);
                db.SaveChanges();
                return Redirect("/Products/Show/" + rev.ProductId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati review-ul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }

        }

    }
}
