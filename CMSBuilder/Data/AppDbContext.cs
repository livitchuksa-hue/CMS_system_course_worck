using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Website> Websites => Set<Website>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageElement> PageElements => Set<PageElement>();
    public DbSet<ElementAction> ElementActions => Set<ElementAction>();
    public DbSet<WebsiteUserRole> WebsiteUserRoles => Set<WebsiteUserRole>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<VisitStatistic> VisitStatistics => Set<VisitStatistic>();
    public DbSet<WebsiteSettings> WebsiteSettings => Set<WebsiteSettings>();
    public DbSet<SiteTheme> SiteThemes => Set<SiteTheme>();
    public DbSet<PageComment> PageComments => Set<PageComment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "cms.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Login).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Website>(e =>
        {
            e.HasOne(x => x.Owner).WithMany(u => u.OwnedWebsites).HasForeignKey(x => x.OwnerId);
        });

        modelBuilder.Entity<WebsiteSettings>(e =>
        {
            e.HasIndex(x => x.WebsiteId).IsUnique();
            e.HasOne(x => x.Website).WithOne(w => w.Settings).HasForeignKey<WebsiteSettings>(x => x.WebsiteId);
        });

        modelBuilder.Entity<SiteTheme>(e =>
        {
            e.HasIndex(x => x.WebsiteId).IsUnique();
            e.HasOne(x => x.Website).WithOne(w => w.Theme).HasForeignKey<SiteTheme>(x => x.WebsiteId);
        });

        modelBuilder.Entity<Page>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.Pages).HasForeignKey(x => x.WebsiteId);
            e.HasIndex(x => new { x.WebsiteId, x.Slug }).IsUnique();
        });

        modelBuilder.Entity<PageElement>(e =>
        {
            e.HasOne(x => x.Page).WithMany(p => p.Elements).HasForeignKey(x => x.PageId);
            e.HasOne(x => x.Parent).WithMany(c => c.Children).HasForeignKey(x => x.ParentElementId);
            e.HasOne(x => x.Action).WithMany(a => a.Elements).HasForeignKey(x => x.ActionId);
        });

        modelBuilder.Entity<ElementAction>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.Actions).HasForeignKey(x => x.WebsiteId);
            e.HasOne(x => x.TargetPage).WithMany().HasForeignKey(x => x.TargetPageId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WebsiteUserRole>(e =>
        {
            e.HasIndex(x => new { x.WebsiteId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<PageComment>(e =>
        {
            e.HasOne(x => x.Page).WithMany().HasForeignKey(x => x.PageId);
            e.HasIndex(x => new { x.PageId, x.ElementId });
        });
    }
}
