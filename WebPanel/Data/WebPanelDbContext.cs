using Microsoft.EntityFrameworkCore;
using WebPanel.Models;

namespace WebPanel.Data;

public class WebPanelDbContext : DbContext
{
    public WebPanelDbContext(DbContextOptions<WebPanelDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ClientSession> ClientSessions { get; set; } = null!;
    public DbSet<CommandHistory> CommandHistory { get; set; } = null!;
    public DbSet<FileTransfer> FileTransfers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ClientSession configuration
        modelBuilder.Entity<ClientSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Hwid).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.OS).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.ConnectedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // CommandHistory configuration
        modelBuilder.Entity<CommandHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.Property(e => e.ExecutedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // FileTransfer configuration
        modelBuilder.Entity<FileTransfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
