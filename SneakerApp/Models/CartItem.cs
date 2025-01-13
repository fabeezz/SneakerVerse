using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public string UserId { get; set; }

        public virtual Product Product { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}