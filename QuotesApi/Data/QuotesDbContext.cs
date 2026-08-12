using Microsoft.EntityFrameworkCore;
using QuotesApi.Models;
using QuotesApi.Domain; 

namespace QuotesApi.Data;

public class QuotesDbContext : DbContext
{
    public QuotesDbContext(DbContextOptions<QuotesDbContext> options) : base(options) { }

    public DbSet<Quote> Quotes => Set<Quote>();
    
    // 1. Added the Collections DbSet
    public DbSet<Collection> Collections { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Your existing Quote configuration
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Author).IsRequired().HasMaxLength(200);
            entity.Property(q => q.Text).IsRequired().HasMaxLength(1000);
        });

        // 2. Added the Collection and CollectionItem configuration
        modelBuilder.Entity<Collection>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(80);
            builder.Property(c => c.OwnerId).IsRequired();

            // Maps the CollectionItem as an Owned Entity (Value Object)
            builder.OwnsMany(c => c.Items, itemBuilder =>
            {
                itemBuilder.WithOwner().HasForeignKey("CollectionId");
                itemBuilder.Property<int>("Id");
                itemBuilder.HasKey("Id");
                itemBuilder.Property(i => i.QuoteId).IsRequired();
                itemBuilder.Property(i => i.AddedAt).IsRequired();
            });
        });
    }
}