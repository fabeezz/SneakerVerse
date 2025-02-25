using Microsoft.EntityFrameworkCore;

namespace SneakerApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<ProductWishlist> ProductWishlists { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PK compus
            modelBuilder.Entity<ProductWishlist>().HasKey(pw => new
            {
                pw.ProductId,
                pw.WishlistId
            });

            // M-M (Product-Wishlist)
            modelBuilder.Entity<ProductWishlist>()
                .HasOne(pw => pw.Product)
                .WithMany(pw => pw.ProductWishlists)
                .HasForeignKey(pw => pw.ProductId);

            modelBuilder.Entity<ProductWishlist>()
                .HasOne(pw => pw.Wishlist)
                .WithMany(pw => pw.ProductWishlists)
                .HasForeignKey(pw => pw.WishlistId);

            // O-M (Product-Category)
            modelBuilder.Entity<Product>()
                .HasOne<Category>(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
        }
    }
}
