using eCommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Offer = eCommerce.Core.Entities.Offer;
using Order = eCommerce.Core.Entities.Order;

namespace eCommerce.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProductQuestion> ProductQuestions { get; set; }
    public DbSet<ProductAnswer> ProductAnswers { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<CartContent> CartContents { get; set; }
    public DbSet<SliderContent> SliderContents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(GetIsDeletedRestriction), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .MakeGenericMethod(entityType.ClrType);

                var filter = method.Invoke(null, Array.Empty<object>());
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter);
            }
        }
        // Category self-reference
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem → ProductVariant
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany(pv => pv.OrderItems)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // CartItem → ProductVariant 
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.ProductVariant)
            .WithMany(pv => pv.CartItems)
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.ProductVariant)
            .WithMany() 
            .HasForeignKey(ci => ci.ProductVariantId);
        
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }

    private static LambdaExpression GetIsDeletedRestriction<T>() where T : BaseEntity
    {
        Expression<Func<T, bool>> filter = x => !x.IsDeleted;
        return filter;
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChanges();
    }
}

// cd ../eCommerce.API dotnet ef migrations add Address --project ../eCommerce.Infrastructure --startup-project
// dotnet ef database update --project ../eCommerce.Infrastructure --startup-project