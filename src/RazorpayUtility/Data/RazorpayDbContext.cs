using Microsoft.EntityFrameworkCore;

namespace RazorpayUtility.Data;

/// <summary>
/// Dedicated Entity Framework DbContext for Razorpay settings and configuration.
/// </summary>
public class RazorpayDbContext : DbContext
{
    public RazorpayDbContext(DbContextOptions<RazorpayDbContext> options) : base(options)
    {
    }

    public DbSet<RazorpaySettingsEntity> RazorpaySettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RazorpaySettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KeyId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.KeySecret).IsRequired().HasMaxLength(255);
            entity.Property(e => e.WebhookSecret).HasMaxLength(255);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(e => e.CompanyName).HasMaxLength(255).HasDefaultValue("Vargshala");
            entity.Property(e => e.ThemeColor).HasMaxLength(20).HasDefaultValue("#009488");
            entity.Property(e => e.SuccessUrl).HasMaxLength(500);
            entity.Property(e => e.CancelUrl).HasMaxLength(500);

            entity.ToTable("RazorpaySettings");
        });
    }
}
