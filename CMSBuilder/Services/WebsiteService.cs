using CMSBuilder.Core;
using CMSBuilder.Data;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services;

public class WebsiteService
{
    public List<Website> GetUserWebsites(int userId)
    {
        using var db = new AppDbContext();
        return db.Websites
            .Include(w => w.Owner)
            .Include(w => w.UserRoles).ThenInclude(ur => ur.Role)
            .Where(w => w.OwnerId == userId || w.UserRoles.Any(ur => ur.UserId == userId))
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    public string GetUserRoleOnWebsite(int userId, Website website) =>
        GetUserRoleNameFromCode(GetUserRoleCode(userId, website));

    public string GetUserRoleCode(int userId, Website website)
    {
        if (website.OwnerId == userId) return "owner";
        using var db = new AppDbContext();
        var role = db.WebsiteUserRoles
            .Include(ur => ur.Role)
            .FirstOrDefault(ur => ur.WebsiteId == website.Id && ur.UserId == userId);
        return role?.Role.Code ?? "viewer";
    }

    public static string GetUserRoleNameFromCode(string code) => code switch
    {
        "owner" => "Владелец",
        "admin" => "Администратор",
        "moderator" => "Модератор",
        _ => "Просмотр"
    };

    public void UpdateSiteTheme(int websiteId, Action<SiteTheme> update)
    {
        using var db = new AppDbContext();
        var theme = db.SiteThemes.FirstOrDefault(t => t.WebsiteId == websiteId);
        if (theme == null)
        {
            theme = new SiteTheme { WebsiteId = websiteId };
            db.SiteThemes.Add(theme);
        }
        update(theme);
        db.SaveChanges();
    }

    public void UpdateExportPath(int websiteId, string exportPath)
    {
        using var db = new AppDbContext();
        var website = db.Websites.Find(websiteId);
        if (website == null) return;
        website.LastExportPath = exportPath;
        website.LastExportedAt = DateTime.UtcNow;
        db.SaveChanges();
    }

    public Website CreateWebsite(int ownerId, string name)
    {
        using var db = new AppDbContext();
        var slug = Slugify(name);
        var ownerRole = db.Roles.First(r => r.Code == "owner");
        var website = new Website
        {
            Name = name,
            Description = string.Empty,
            Slug = slug,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            Settings = new WebsiteSettings { SiteTitle = name, BaseUrl = slug },
            Theme = new SiteTheme()
        };
        db.Websites.Add(website);
        db.SaveChanges();

        db.WebsiteUserRoles.Add(new WebsiteUserRole
        {
            WebsiteId = website.Id,
            UserId = ownerId,
            RoleId = ownerRole.Id
        });

        var home = new Page
        {
            WebsiteId = website.Id,
            Name = "Главная",
            Slug = "index",
            IsHome = true,
            ShowInNav = true,
            SortOrder = 0
        };
        db.Pages.Add(home);
        db.SaveChanges();
        return website;
    }

    public Website? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.Websites
            .Include(w => w.Theme)
            .Include(w => w.Settings)
            .FirstOrDefault(w => w.Id == id);
    }

    public void UpdateWebsiteCore(int websiteId, string name, string description, string slug)
    {
        using var db = new AppDbContext();
        var website = db.Websites.FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return;

        website.Name = name.Trim();
        website.Description = description?.Trim() ?? string.Empty;
        website.Slug = string.IsNullOrWhiteSpace(slug) ? Slugify(name) : Slugify(slug);
        db.SaveChanges();
    }

    public void UpdateWebsiteSettings(int websiteId, Action<WebsiteSettings> update)
    {
        using var db = new AppDbContext();
        var settings = db.WebsiteSettings.FirstOrDefault(s => s.WebsiteId == websiteId);
        if (settings == null)
        {
            settings = new WebsiteSettings { WebsiteId = websiteId };
            db.WebsiteSettings.Add(settings);
        }

        update(settings);
        db.SaveChanges();
    }

    public WebsiteSettings? GetSettings(int websiteId)
    {
        using var db = new AppDbContext();
        return db.WebsiteSettings.FirstOrDefault(s => s.WebsiteId == websiteId);
    }

    public static string Slugify(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = string.Concat(slug.Select(c => char.IsLetterOrDigit(c) ? c : '-'));
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return string.IsNullOrWhiteSpace(slug) ? "site" : slug.Trim('-');
    }
}
