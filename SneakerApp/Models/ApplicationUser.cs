using Microsoft.AspNetCore.Identity;

// PASUL 1: USERI SI ROLURI

namespace SneakerApp.Models
{
    public class ApplicationUser : IdentityUser
    {

        // Pasul 6: USERI SI ROLURI
        // un user poate posta mai multe review uri

        public virtual ICollection<Review>? Reviews { get; set; }

        // un user poate posta mai multe produse

        public virtual ICollection <Product>? Products { get; set; }

    }
}
