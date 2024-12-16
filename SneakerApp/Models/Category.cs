using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string CategoryName { get; set; }

        // dintr o categorie fac parte mai multe articole
        public virtual ICollection<Product>? Products { get; set; }
    }
}