using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Pretul este obligatoriu!")]
        public float Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu!")]
        public int Stock { get; set; }

        public int Rating { get; set; }

        // CATEGORIES

        [Required(ErrorMessage = "Categoria este obligatorie!")]
        // FK
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // REVIEWS

        public virtual ICollection<Review>? Reviews { get; set; }

        public virtual ICollection<ProductWishlist>? ProductWishlists { get; set; }

        // PASUL 6 - useri si roluri
        public string? UserId { get; set; }

        // un articol postat de catre un user
        public virtual ApplicationUser? User { get; set; }
    }
}