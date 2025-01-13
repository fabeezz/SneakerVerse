using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SneakerApp.Models;

namespace SneakerApp.Data
{
    // PASUL 3: useri si roluri
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<ProductWishlist> ProductWishlists { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

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
                .WithMany(p => p.ProductWishlists)
                .HasForeignKey(pw => pw.ProductId);

            modelBuilder.Entity<ProductWishlist>()
                .HasOne(pw => pw.Wishlist)
                .WithMany(w => w.ProductWishlists)
                .HasForeignKey(pw => pw.WishlistId);

            // O-M (Category-Product)
            modelBuilder.Entity<Product>()
                .HasOne<Category>(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // O-M (Product-Review)
            modelBuilder.Entity<Review>()
                .HasOne<Product>(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany() // Assuming a product can be in many carts, but we don't need a navigation property back to CartItem
                .HasForeignKey(c => c.ProductId);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany() // Assuming a user can have many cart items, but we don't need a navigation property back to CartItem
                .HasForeignKey(c => c.UserId);

            //// Review-ApplicationUser relationship
            //modelBuilder.Entity<Review>()
            //    .HasOne<ApplicationUser>(r => r.User)
            //    .WithMany()
            //    .HasForeignKey(r => r.UserId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        }
    }
}
