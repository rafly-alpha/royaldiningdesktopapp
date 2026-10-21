using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Models;
using System.Configuration;

namespace RoyalDining.Desktop.Data;

public class AppDbContext : DbContext
{
    // =============================================
    // Connection String dari App.config
    // =============================================
    private static string ConnectionString
    {
        get
        {
            var conn = ConfigurationManager.ConnectionStrings["RoyalDiningDB"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException(
                    "Connection string 'RoyalDiningDB' tidak ditemukan di App.config");
            return conn;
        }
    }

    // =============================================
    // DbSets
    // =============================================
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // =============================================
    // OnConfiguring
    // =============================================
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }

    // =============================================
    // Model Configuration
    // =============================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique Category Name
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // User -> Orders (1:N, cascade)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> OrderItems (1:N, cascade)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Menu -> OrderItems (1:N, restrict)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Menu)
            .WithMany(m => m.OrderItems)
            .HasForeignKey(oi => oi.MenuId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category -> Menus (1:N, restrict)
        modelBuilder.Entity<Menu>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Menus)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>().ToTable(tb => tb.HasTrigger("TR_Users_UpdatedAt"));
        modelBuilder.Entity<Category>().ToTable(tb => tb.HasTrigger("TR_Categories_UpdatedAt"));
        modelBuilder.Entity<Menu>().ToTable(tb => tb.HasTrigger("TR_Menus_UpdatedAt"));
        modelBuilder.Entity<Order>().ToTable(tb => tb.HasTrigger("TR_Orders_UpdatedAt"));
        modelBuilder.Entity<OrderItem>().ToTable(tb => tb.HasTrigger("TR_OrderItems_UpdatedAt"));

    }
}