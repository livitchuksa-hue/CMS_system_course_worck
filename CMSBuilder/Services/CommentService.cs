using CMSBuilder.Data;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services;

public class CommentService
{
    public List<PageComment> GetForBlock(int pageId, int elementId)
    {
        using var db = new AppDbContext();
        return db.PageComments
            .Where(c => c.PageId == pageId && c.ElementId == elementId)
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }

    public List<PageComment> GetForWebsite(int websiteId)
    {
        using var db = new AppDbContext();
        return db.PageComments
            .Include(c => c.Page)
            .Where(c => c.Page.WebsiteId == websiteId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public List<PageComment> GetForPage(int pageId)
    {
        using var db = new AppDbContext();
        return db.PageComments
            .Where(c => c.PageId == pageId)
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }

    public PageComment Add(int pageId, int elementId, string authorName, string text)
    {
        using var db = new AppDbContext();
        var comment = new PageComment
        {
            PageId = pageId,
            ElementId = elementId,
            AuthorName = string.IsNullOrWhiteSpace(authorName) ? "Гость" : authorName.Trim(),
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.PageComments.Add(comment);
        db.SaveChanges();
        return comment;
    }

    public void Delete(int commentId)
    {
        using var db = new AppDbContext();
        var c = db.PageComments.Find(commentId);
        if (c != null)
        {
            db.PageComments.Remove(c);
            db.SaveChanges();
        }
    }
}
