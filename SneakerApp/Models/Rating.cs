using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required(ErrorMessage = "Scorul este obligatoriu")]
        public int Score { get; set; }

        // fk cu product
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // fk cu user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
