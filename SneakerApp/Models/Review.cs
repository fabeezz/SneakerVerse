﻿using System.ComponentModel.DataAnnotations;

namespace SneakerApp.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }

        public int? Score { get; set; }

        public DateTime Date { get; set; }

        // fk cu product
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // PASUL 6 - useri si roluri
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}