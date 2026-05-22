using CMSBuilder.Helpers;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Data;

public static class DbInitializer
{
    public static void Initialize()
    {
        using var db = new AppDbContext();
        EnsureDatabaseReady(db);
        Seed(db);
    }

    private static void EnsureDatabaseReady(AppDbContext db)
    {
        var recreate = false;

        try
        {
            if (!db.Database.CanConnect())
                recreate = true;
            else if (!IsSchemaValid(db))
                recreate = true;
        }
        catch
        {
            recreate = true;
        }

        if (recreate)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        else
        {
            db.Database.EnsureCreated();
        }
    }

    private static bool IsSchemaValid(AppDbContext db)
    {
        try
        {
            _ = db.Roles.Any();
            db.Database.ExecuteSqlRaw("SELECT 1 FROM PageElements LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM WebsiteSettings LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT PageWidth FROM WebsiteSettings LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM SiteThemes LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM PageComments LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT TargetElementId FROM ElementActions LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT LastExportPath FROM Websites LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM CardElementData LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM VideoPlayerElementData LIMIT 1");
            db.Database.ExecuteSqlRaw("SELECT 1 FROM CommentsViewerElementData LIMIT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void Seed(AppDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "Владелец", Code = "owner" },
                new Role { Name = "Администратор", Code = "admin" },
                new Role { Name = "Модератор", Code = "moderator" },
                new Role { Name = "Просмотр", Code = "viewer" });
            db.SaveChanges();
        }

        if (db.Users.Any(u => u.Login == "admin"))
            return;

        var ownerRole = db.Roles.First(r => r.Code == "owner");

        var user = new User
        {
            Login = "admin",
            Email = "admin@cms.local",
            PasswordHash = PasswordHasher.Hash("admin123"),
            RegisteredAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();

        var website = new Website
        {
            Name = "Демо сайт",
            Description = "Пример для курсовой",
            Slug = "demo-site",
            OwnerId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.Websites.Add(website);
        db.SaveChanges();

        db.WebsiteSettings.Add(new WebsiteSettings
        {
            WebsiteId = website.Id,
            SiteTitle = "Демо сайт",
            BaseUrl = "demo-site",
            PageWidth = 960,
            PageHeight = 640,
            PageBackgroundColor = "#FFFFFF"
        });
        db.SiteThemes.Add(new SiteTheme { WebsiteId = website.Id });
        db.WebsiteUserRoles.Add(new WebsiteUserRole
        {
            WebsiteId = website.Id,
            UserId = user.Id,
            RoleId = ownerRole.Id
        });
        db.SaveChanges();

        var home = new Page
        {
            WebsiteId = website.Id,
            Name = "Главная",
            Slug = "index",
            IsHome = true,
            ShowInNav = true,
            SortOrder = 0
        };
        var about = new Page
        {
            WebsiteId = website.Id,
            Name = "О нас",
            Slug = "about",
            IsHome = false,
            ShowInNav = true,
            SortOrder = 1
        };
        db.Pages.AddRange(home, about);
        db.SaveChanges();

        db.PageElements.Add(new PageElement
        {
            PageId = home.Id,
            Type = Models.Enums.ElementType.Header,
            SortOrder = 0,
            X = 40,
            Y = 40,
            Width = null,
            Height = null,
            PropertiesJson = ElementJsonHelper.SerializeProperties(new Models.Dto.ElementPropertiesDto
            {
                Text = "Добро пожаловать",
                Level = "h1"
            }),
            StyleJson = ElementJsonHelper.SerializeStyle(new Models.Dto.ElementStyleDto
            {
                FontSize = "32px",
                Color = "#1A2420"
            })
        });
        db.SaveChanges();
    }
}
