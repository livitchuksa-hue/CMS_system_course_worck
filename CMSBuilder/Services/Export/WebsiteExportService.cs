using CMSBuilder.Data;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CMSBuilder.Services.Export;

public class WebsiteExportService
{
    private readonly HtmlGeneratorService _html = new();
    private readonly CssGeneratorService _css = new();

    public string ExportWebsite(int websiteId)
    {
        using var db = new AppDbContext();
        var website = db.Websites
            .Include(w => w.Theme)
            .Include(w => w.Settings)
            .Include(w => w.Pages)
            .FirstOrDefault(w => w.Id == websiteId)
            ?? throw new InvalidOperationException("Сайт не найден.");

        var theme = website.Theme ?? new SiteTheme();
        var settings = website.Settings;
        var pages = website.Pages.OrderBy(p => p.SortOrder).ToList();
        var allElements = db.PageElements
            .Include(e => e.Action)
            .Where(e => pages.Select(p => p.Id).Contains(e.PageId))
            .ToList();

        ComplexElementService.HydrateAll(allElements);

        var folder = FileHelper.GetWebsiteExportFolder(website.Id, website.Slug);

        var allComments = db.PageComments.Where(c => pages.Select(p => p.Id).Contains(c.PageId)).ToList();

        foreach (var page in pages)
        {
            var pageElements = allElements
                .Where(e => e.PageId == page.Id && e.ParentElementId == null && !IsFormBoundField(e))
                .ToList();
            var pageAll = allElements.Where(e => e.PageId == page.Id).ToList();
            var pageComments = allComments.Where(c => c.PageId == page.Id).ToList();
            var fileName = page.IsHome ? "index.html" : $"{page.Slug}.html";
            var html = _html.GeneratePageHtml(page, pageElements, pageAll, pages, theme, pageComments);
            File.WriteAllText(Path.Combine(folder, fileName), html);
        }

        var css = _css.GenerateSiteCss(theme, settings, allElements);
        File.WriteAllText(Path.Combine(folder, "styles.css"), css);

        var js = GenerateJavaScript(allElements, pages, allComments);
        File.WriteAllText(Path.Combine(folder, "script.js"), js);
        File.WriteAllText(Path.Combine(folder, "comments-data.json"),
            System.Text.Json.JsonSerializer.Serialize(allComments.Select(c => new
            {
                c.Id, c.PageId, c.ElementId, c.AuthorName, c.Text,
                CreatedAt = c.CreatedAt.ToString("o")
            })));

        return folder;
    }

    private static bool IsFormBoundField(PageElement e)
    {
        if (e.Type is not (ElementType.Input or ElementType.TextArea or ElementType.Checkbox or ElementType.Button))
            return false;
        var props = ElementJsonHelper.DeserializeProperties(e.PropertiesJson);
        return props.FormElementId.HasValue;
    }

    private static string GenerateJavaScript(List<PageElement> elements, List<Page> pages,
        List<PageComment> comments)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("function navigateToPage(file) { window.location.href = file; }");
        sb.AppendLine();
        sb.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
        sb.AppendLine("  document.querySelectorAll('.comment-submit').forEach(function(btn) {");
        sb.AppendLine("    btn.addEventListener('click', function() {");
        sb.AppendLine("      var blockId = btn.getAttribute('data-block');");
        sb.AppendLine("      var section = document.querySelector('[data-comments-block=\"' + blockId + '\"]');");
        sb.AppendLine("      if (!section) return;");
        sb.AppendLine("      var author = section.querySelector('.comment-author').value.trim() || 'Гость';");
        sb.AppendLine("      var text = section.querySelector('.comment-text').value.trim();");
        sb.AppendLine("      if (!text) return;");
        sb.AppendLine("      var list = section.querySelector('.comments-list');");
        sb.AppendLine("      var article = document.createElement('article');");
        sb.AppendLine("      article.className = 'comment-item';");
        sb.AppendLine("      article.innerHTML = '<strong>' + escapeHtml(author) + '</strong><p>' + escapeHtml(text) + '</p>';");
        sb.AppendLine("      list.appendChild(article);");
        sb.AppendLine("      section.querySelector('.comment-text').value = '';");
        sb.AppendLine("      var key = 'cms_comments_' + blockId;");
        sb.AppendLine("      var stored = JSON.parse(localStorage.getItem(key) || '[]');");
        sb.AppendLine("      stored.push({ author: author, text: text, at: new Date().toISOString() });");
        sb.AppendLine("      localStorage.setItem(key, JSON.stringify(stored));");
        sb.AppendLine("      syncCommentToViewers(blockId, author, text);");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("  document.querySelectorAll('[data-comments-block]').forEach(function(section) {");
        sb.AppendLine("    var blockId = section.getAttribute('data-comments-block');");
        sb.AppendLine("    var key = 'cms_comments_' + blockId;");
        sb.AppendLine("    var stored = JSON.parse(localStorage.getItem(key) || '[]');");
        sb.AppendLine("    var list = section.querySelector('.comments-list');");
        sb.AppendLine("    stored.forEach(function(c) {");
        sb.AppendLine("      var article = document.createElement('article');");
        sb.AppendLine("      article.className = 'comment-item';");
        sb.AppendLine("      article.innerHTML = '<strong>' + escapeHtml(c.author) + '</strong><p>' + escapeHtml(c.text) + '</p>';");
        sb.AppendLine("      list.appendChild(article);");
        sb.AppendLine("      syncCommentToViewers(blockId, c.author, c.text);");
        sb.AppendLine("    });");
        sb.AppendLine("  });");
        sb.AppendLine("});");
        sb.AppendLine("function syncCommentToViewers(blockId, author, text) {");
        sb.AppendLine("  document.querySelectorAll('[data-comments-viewer][data-comments-source=\"' + blockId + '\"]').forEach(function(viewer) {");
        sb.AppendLine("    var list = viewer.querySelector('.comments-list');");
        sb.AppendLine("    if (!list) return;");
        sb.AppendLine("    var empty = list.querySelector('.comments-empty');");
        sb.AppendLine("    if (empty) empty.remove();");
        sb.AppendLine("    var article = document.createElement('article');");
        sb.AppendLine("    article.className = 'comment-item';");
        sb.AppendLine("    article.innerHTML = '<strong>' + escapeHtml(author) + '</strong><p>' + escapeHtml(text) + '</p>';");
        sb.AppendLine("    list.appendChild(article);");
        sb.AppendLine("  });");
        sb.AppendLine("}");
        sb.AppendLine("function escapeHtml(s) {");
        sb.AppendLine("  return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/\"/g,'&quot;');");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("function collectFormData(formId) {");
        sb.AppendLine("  var form = document.getElementById('el-' + formId);");
        sb.AppendLine("  if (!form) return {};");
        sb.AppendLine("  var data = {};");
        sb.AppendLine("  form.querySelectorAll('input, textarea, select').forEach(function(el) {");
        sb.AppendLine("    if (el.name) data[el.name] = el.type === 'checkbox' ? el.checked : el.value;");
        sb.AppendLine("  });");
        sb.AppendLine("  document.querySelectorAll('[data-form-id=\"' + formId + '\"]').forEach(function(el) {");
        sb.AppendLine("    if (el.name && !(el.name in data)) data[el.name] = el.type === 'checkbox' ? el.checked : el.value;");
        sb.AppendLine("  });");
        sb.AppendLine("  return data;");
        sb.AppendLine("}");

        var actions = elements.Where(e => e.Action != null).Select(e => e.Action!).DistinctBy(a => a.Id);
        foreach (var action in actions)
        {
            if (action.ActionType == ActionType.CustomScript && !string.IsNullOrWhiteSpace(action.CustomJavaScript))
            {
                sb.AppendLine($"function runAction_{action.Id}() {{");
                sb.AppendLine(action.CustomJavaScript);
                sb.AppendLine("}");
            }
            if (action.ActionType == ActionType.OpenPopup)
            {
                sb.AppendLine($"function openPopup_{action.Id}() {{");
                sb.AppendLine($"  alert({System.Text.Json.JsonSerializer.Serialize(action.PopupHtml ?? "Popup")});");
                sb.AppendLine("}");
            }
        }

        var formIds = elements
            .Where(e => e.Type == ElementType.Form)
            .Select(e => e.Id)
            .Distinct();
        foreach (var formId in formIds)
        {
            sb.AppendLine($"function submitForm_{formId}(ev) {{");
            sb.AppendLine("  if (ev) ev.preventDefault();");
            sb.AppendLine($"  var data = collectFormData({formId});");
            sb.AppendLine("  console.log('Form submit', data);");
            sb.AppendLine("  alert('Форма отправлена!\\n' + JSON.stringify(data, null, 2));");
            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}
