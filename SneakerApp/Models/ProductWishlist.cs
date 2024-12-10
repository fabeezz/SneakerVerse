using System.ComponentModel.DataAnnotations.Schema;

namespace SneakerApp.Models
{
    public class ProductWishlist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProductId { get; set; }

        public int? WishlistId { get; set; }

        public virtual Product? Product { get; set; }

        public virtual Wishlist? Wishlist { get; set; }

        public DateTime WishlistDate { get; set; }
    }
}