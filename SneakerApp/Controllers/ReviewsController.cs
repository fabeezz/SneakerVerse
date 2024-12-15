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

        // EDIT
        public IActionResult Edit(int id)
        {
            Review comm = db.Reviews.Find(id);

            return View(comm);
        }


        [HttpPost]
        public IActionResult Edit(int id, Review requestReview)
        {
            Review comm = db.Reviews.Find(id);

            if (ModelState.IsValid)
            {

                comm.Content = requestReview.Content;

                db.SaveChanges();

                return Redirect("/Articles/Show/" + comm.ReviewId);
            }
            else
            {
                return View(requestReview);
            }

        }

        // DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Review comm = db.Reviews.Find(id);
            db.Reviews.Remove(comm);
            db.SaveChanges();
            return Redirect("/Products/Show/" + comm.ProductId);
        }
        
    }
}
