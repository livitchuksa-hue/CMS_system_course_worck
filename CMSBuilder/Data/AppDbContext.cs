using CMSBuilder.Models;
using CMSBuilder.Models.ComplexElements;
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
    public DbSet<CardElementData> CardElementData => Set<CardElementData>();
    public DbSet<VideoPlayerElementData> VideoPlayerElementData => Set<VideoPlayerElementData>();
    public DbSet<CommentsViewerElementData> CommentsViewerElementData => Set<CommentsViewerElementData>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Login).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        // SQL Server: несколько CASCADE от Website к дочерним таблицам даёт «multiple cascade paths»
        // (например Website → Pages → … и Website → ElementActions → TargetPage → Pages).
        // Удаление сайта выполняется вручную в WebsiteService.DeleteWebsite.
        modelBuilder.Entity<Website>(e =>
        {
            e.HasOne(x => x.Owner).WithMany(u => u.OwnedWebsites).HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WebsiteSettings>(e =>
        {
            e.HasIndex(x => x.WebsiteId).IsUnique();
            e.HasOne(x => x.Website).WithOne(w => w.Settings).HasForeignKey<WebsiteSettings>(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SiteTheme>(e =>
        {
            e.HasIndex(x => x.WebsiteId).IsUnique();
            e.HasOne(x => x.Website).WithOne(w => w.Theme).HasForeignKey<SiteTheme>(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Page>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.Pages).HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.WebsiteId, x.Slug }).IsUnique();
        });

        modelBuilder.Entity<PageElement>(e =>
        {
            e.HasOne(x => x.Page).WithMany(p => p.Elements).HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent).WithMany(c => c.Children).HasForeignKey(x => x.ParentElementId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Action).WithMany(a => a.Elements).HasForeignKey(x => x.ActionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ElementAction>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.Actions).HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.TargetPage).WithMany().HasForeignKey(x => x.TargetPageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WebsiteUserRole>(e =>
        {
            e.HasIndex(x => new { x.WebsiteId, x.UserId }).IsUnique();
            e.HasOne(x => x.Website).WithMany(w => w.UserRoles).HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.User).WithMany(u => u.WebsiteRoles).HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Role).WithMany(r => r.WebsiteUserRoles).HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invitation>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.Invitations).HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VisitStatistic>(e =>
        {
            e.HasOne(x => x.Website).WithMany(w => w.VisitStatistics).HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PageComment>(e =>
        {
            e.HasOne(x => x.Page).WithMany().HasForeignKey(x => x.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.PageId, x.ElementId });
        });

        modelBuilder.Entity<CardElementData>(e =>
        {
            e.HasKey(x => x.PageElementId);
            e.HasOne(x => x.PageElement).WithOne().HasForeignKey<CardElementData>(x => x.PageElementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VideoPlayerElementData>(e =>
        {
            e.HasKey(x => x.PageElementId);
            e.HasOne(x => x.PageElement).WithOne().HasForeignKey<VideoPlayerElementData>(x => x.PageElementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentsViewerElementData>(e =>
        {
            e.HasKey(x => x.PageElementId);
            e.HasOne(x => x.PageElement).WithOne().HasForeignKey<CommentsViewerElementData>(x => x.PageElementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
