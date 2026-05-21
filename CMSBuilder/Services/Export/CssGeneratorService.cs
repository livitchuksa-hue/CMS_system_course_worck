using System.Text;
using CMSBuilder.Helpers;
using CMSBuilder.Models;

namespace CMSBuilder.Services.Export;

public class CssGeneratorService
{
    public string GenerateSiteCss(SiteTheme theme, WebsiteSettings? settings, IEnumerable<PageElement> allElements)
    {
        var pageWidth = settings?.PageWidth > 0 ? settings.PageWidth : ExportLayoutHelper.CanvasWidth;
        var pageHeight = settings?.PageHeight > 0 ? settings.PageHeight : ExportLayoutHelper.CanvasHeight;
        var pageBg = settings?.PageBackgroundColor ?? "#FFFFFF";

        var css = new StringBuilder();
        css.AppendLine("* { box-sizing: border-box; }");
        css.AppendLine(":root { --primary: " + theme.PrimaryColor + "; --secondary: " + theme.SecondaryColor + "; --accent: " + theme.AccentColor + "; }");
        css.AppendLine("body { margin: 0; font-family: " + theme.FontFamily + "; color: #1A2420; background: #ecefed; }");
        css.AppendLine(".btn-primary { background: var(--primary); color: #fff; border-radius: " + theme.ButtonRadius + "px; }");
        css.AppendLine(".navbar-el a { color: var(--primary); }");
        css.AppendLine(".card-el { background: #fff; border-radius: 10px; overflow: hidden; cursor: pointer; box-shadow: 0 2px 8px rgba(0,0,0,.08); display: flex; flex-direction: column; }");
        css.AppendLine(".card-el:hover { box-shadow: 0 4px 14px rgba(0,0,0,.12); }");
        css.AppendLine(".card-img { width: 100%; height: 140px; object-fit: cover; display: block; }");
        css.AppendLine(".card-body { padding: 12px; }");
        css.AppendLine(".card-title { margin: 0 0 6px; color: var(--primary); font-size: 1.1rem; }");
        css.AppendLine(".card-desc { margin: 0; color: var(--secondary); font-size: 0.9rem; }");
        css.AppendLine(".container-label { font-size: 12px; color: var(--secondary); width: 100%; }");
        css.AppendLine(".page {");
        css.AppendLine("  position: relative;");
        css.AppendLine("  width: " + pageWidth.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px;");
        css.AppendLine("  min-height: " + pageHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px;");
        css.AppendLine("  margin: 0 auto;");
        css.AppendLine("  background: " + pageBg + ";");
        css.AppendLine("  overflow: hidden;");
        css.AppendLine("}");
        css.AppendLine(".el { position: absolute; margin: 0; }");
        css.AppendLine(".btn-primary { border: none; cursor: pointer; padding: 12px 24px; }");
        css.AppendLine(".navbar-el { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; border-bottom: 1px solid #e0e0e0; }");
        css.AppendLine(".navbar-el a { margin-left: 12px; text-decoration: none; }");
        css.AppendLine(".gallery-el { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; }");
        css.AppendLine(".gallery-el img { width: 100%; border-radius: 8px; object-fit: cover; }");
        css.AppendLine(".container-box { border: 1px dashed #ccc; padding: 16px; border-radius: 8px; }");
        css.AppendLine(".divider-el { border-top: 1px solid #ddd; min-height: 1px; }");
        css.AppendLine(".spacer-el { min-height: 24px; }");
        css.AppendLine(".navbar-logo-img { max-height: 36px; max-width: 140px; object-fit: contain; }");
        css.AppendLine(".form-box { display: flex; flex-direction: column; gap: 8px; }");
        css.AppendLine(".form-title { font-weight: 600; margin-bottom: 8px; }");
        css.AppendLine(".form-field { position: relative !important; left: 0 !important; top: 0 !important; width: 100% !important; }");
        css.AppendLine(".checkbox-el { display: flex; align-items: center; gap: 8px; cursor: pointer; }");
        css.AppendLine(".checkbox-switch input { appearance: none; width: 0; height: 0; opacity: 0; }");
        css.AppendLine(".checkbox-rounded input { border-radius: 50%; }");
        css.AppendLine(".comments-block { display: flex; flex-direction: column; gap: 10px; padding: 12px; border: 1px solid #e0e0e0; border-radius: 8px; }");
        css.AppendLine(".comments-list { display: flex; flex-direction: column; gap: 8px; max-height: 200px; overflow-y: auto; }");
        css.AppendLine(".comment-item { background: #f4f6f5; padding: 8px; border-radius: 6px; }");
        css.AppendLine(".comments-form { display: flex; flex-direction: column; gap: 8px; }");
        css.AppendLine(".comments-form textarea { min-height: 60px; }");
        css.AppendLine(".faq-el details { margin-bottom: 8px; }");
        css.AppendLine(".footer-el { text-align: center; padding: 16px; }");

        foreach (var el in allElements)
            css.AppendLine("#el-" + el.Id + " { " + ExportLayoutHelper.BuildElementCssRules(el) + " }");

        return css.ToString();
    }
}
