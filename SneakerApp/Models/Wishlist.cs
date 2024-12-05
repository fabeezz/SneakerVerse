using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele wishlist-ului este obligatoriu!")]
        public string Name { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<ProductWishlist>? ProductWishlists { get; set; }
    }
}
