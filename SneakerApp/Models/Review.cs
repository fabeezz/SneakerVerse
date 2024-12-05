using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // PASUL 6 - useri si roluri
        public virtual ApplicationUser User { get; set; }
    }
}
