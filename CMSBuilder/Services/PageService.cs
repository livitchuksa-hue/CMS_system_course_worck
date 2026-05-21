using CMSBuilder.Data;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services;

public class PageService
{
    public List<Page> GetPages(int websiteId)
    {
        using var db = new AppDbContext();
        return db.Pages.Where(p => p.WebsiteId == websiteId).OrderBy(p => p.SortOrder).ToList();
    }

    public Page CreatePage(int websiteId, string name)
    {
        using var db = new AppDbContext();
        var maxOrder = db.Pages.Where(p => p.WebsiteId == websiteId).Select(p => (int?)p.SortOrder).Max() ?? -1;
        var slug = name.Trim().ToLowerInvariant().Replace(' ', '-');
        var page = new Page
        {
            WebsiteId = websiteId,
            Name = name,
            Slug = slug,
            SortOrder = maxOrder + 1,
            ShowInNav = true
        };
        db.Pages.Add(page);
        db.SaveChanges();
        return page;
    }

    public Page? GetById(int pageId)
    {
        using var db = new AppDbContext();
        return db.Pages.FirstOrDefault(p => p.Id == pageId);
    }

    public void UpdatePage(Page page)
    {
        using var db = new AppDbContext();
        var existing = db.Pages.FirstOrDefault(p => p.Id == page.Id);
        if (existing == null) return;

        existing.Name = page.Name.Trim();
        existing.Slug = string.IsNullOrWhiteSpace(page.Slug)
            ? page.Name.Trim().ToLowerInvariant().Replace(' ', '-')
            : page.Slug.Trim().ToLowerInvariant();
        existing.ShowInNav = page.ShowInNav;
        db.SaveChanges();
    }
}
