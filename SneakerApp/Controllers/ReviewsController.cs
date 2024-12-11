using Microsoft.AspNetCore.Mvc;
using SneakerApp.Data;
using SneakerApp.Models;

namespace SneakerApp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext db;
        public ReviewsController(ApplicationDbContext context)
        {
            db = context;
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
