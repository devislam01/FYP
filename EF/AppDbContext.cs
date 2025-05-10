using Microsoft.EntityFrameworkCore;

namespace DemoFYP.EF;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Usertoken> Usertokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=elvinlam0814;database=secondlifeunimas_db", ServerVersion.Parse("8.0.33-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("admin");

            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("userID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("createdDateTime");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("deletedBy");
            entity.Property(e => e.DeletedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("deletedDateTime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(45)
                .HasColumnName("password");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("updatedDateTime");
            entity.Property(e => e.UserLevel).HasColumnName("userLevel");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("order");

            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("orderID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("createdDateTime");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("deletedBy");
            entity.Property(e => e.DeletedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("deletedDateTime");
            entity.Property(e => e.Feedback)
                .HasMaxLength(255)
                .HasColumnName("feedback");
            entity.Property(e => e.PaymentId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("paymentID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("productID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Status)
                .HasMaxLength(45)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount).HasColumnName("totalAmount");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("updatedDateTime");
            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("userID");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity.ToTable("payment");

            entity.Property(e => e.PaymentId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("paymentID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("createdDateTime");
            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("orderID");
            entity.Property(e => e.Status)
                .HasMaxLength(45)
                .HasColumnName("status");
            entity.Property(e => e.TotalPaidAmount).HasColumnName("totalPaidAmount");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("updatedDateTime");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product");

            entity.HasIndex(e => e.CategoryId, "categoryID_idx");

            entity.HasIndex(e => e.ProductId, "productID_UNIQUE").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("productID");
            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("createdDateTime");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("deletedBy");
            entity.Property(e => e.DeletedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("deletedDateTime");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.ProductCondition)
                .HasMaxLength(45)
                .HasColumnName("productCondition");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(45)
                .HasColumnName("productDescription");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(45)
                .HasColumnName("productImage");
            entity.Property(e => e.ProductName)
                .HasMaxLength(45)
                .HasColumnName("productName");
            entity.Property(e => e.ProductPrice).HasColumnName("productPrice");
            entity.Property(e => e.ProductRating).HasColumnName("productRating");
            entity.Property(e => e.StockQty).HasColumnName("stockQty");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("updatedDateTime");
            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("userID");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("categoryID");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("product_category");

            entity.HasIndex(e => e.CategoryId, "categoryID_UNIQUE").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(45)
                .HasColumnName("categoryName");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.UserId, "userID_UNIQUE").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("userID");
            entity.Property(e => e.Address)
                .HasMaxLength(45)
                .HasColumnName("address");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("createdDateTime");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("deletedBy");
            entity.Property(e => e.DeletedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("deletedDateTime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Password)
                .HasMaxLength(45)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(45)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.RatingMark).HasColumnName("ratingMark");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDateTime)
                .HasColumnType("datetime")
                .HasColumnName("updatedDateTime");
            entity.Property(e => e.UserGender)
                .HasMaxLength(45)
                .HasColumnName("userGender");
            entity.Property(e => e.UserLevel).HasColumnName("userLevel");
            entity.Property(e => e.UserName)
                .HasMaxLength(45)
                .HasColumnName("userName");
        });

        modelBuilder.Entity<Usertoken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PRIMARY");

            entity.ToTable("usertoken");

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.AccessToken).HasMaxLength(512);
            entity.Property(e => e.AccessTokenExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("AccessToken_ExpiresAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.RefreshToken).HasMaxLength(256);
            entity.Property(e => e.RefreshTokenExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("RefreshToken_ExpiresAt");
            entity.Property(e => e.UserId)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("UserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
