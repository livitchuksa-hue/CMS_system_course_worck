using System.Net;
using System.Text;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Models.Dto;
using CMSBuilder.Models.Enums;

namespace CMSBuilder.Services.Export;

public class HtmlGeneratorService
{
    public string GeneratePageHtml(Page page, List<PageElement> rootElements, List<PageElement> allPageElements,
        List<Page> allPages, SiteTheme theme, List<PageComment> comments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"ru\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.AppendLine($"  <title>{WebUtility.HtmlEncode(page.Name)}</title>");
        sb.AppendLine("  <link rel=\"stylesheet\" href=\"styles.css\" />");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"  <div class=\"page\" data-page-id=\"{page.Id}\">");

        foreach (var el in rootElements.OrderBy(e => e.SortOrder))
            sb.AppendLine(RenderElement(el, allPageElements, allPages, comments, 4));

        sb.AppendLine("  </div>");
        sb.AppendLine("  <script src=\"script.js\"></script>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static string Pos(PageElement el) =>
        "style=\"" + ExportLayoutHelper.BuildInlinePositionStyle(el) + "\"";

    private static string RelativePos(PageElement el) =>
        "style=\"" + ExportLayoutHelper.BuildRelativePositionStyle(el) + "\"";

    private string RenderElement(PageElement el, List<PageElement> all, List<Page> pages,
        List<PageComment> comments, int indent, bool relativePosition = false)
    {
        var pad = new string(' ', indent);
        var props = ElementJsonHelper.DeserializeProperties(el.PropertiesJson);
        var idAttr = $"id=\"el-{el.Id}\"";
        var pos = relativePosition ? RelativePos(el) : Pos(el);

        return el.Type switch
        {
            ElementType.Text => $"{pad}<p {idAttr} class=\"el\" {pos}>{WebUtility.HtmlEncode(props.Text ?? "")}</p>",
            ElementType.Header => $"{pad}<{props.Level ?? "h2"} {idAttr} class=\"el\" {pos}>{WebUtility.HtmlEncode(props.Text ?? "")}</{props.Level ?? "h2"}>",
            ElementType.Image => $"{pad}<img {idAttr} class=\"el\" {pos} src=\"{WebUtility.HtmlEncode(props.Src ?? "")}\" alt=\"{WebUtility.HtmlEncode(props.Alt ?? "")}\" />",
            ElementType.Divider => $"{pad}<div {idAttr} class=\"el divider-el\" {pos}></div>",
            ElementType.Spacer => $"{pad}<div {idAttr} class=\"el spacer-el\" {pos}></div>",
            ElementType.Container => RenderContainer(el, props, all, pages, comments, indent),
            ElementType.Card => RenderCard(el, props, pages, indent, relativePosition),
            ElementType.Button => RenderButton(el, props, pages, indent, relativePosition),
            ElementType.Link => $"{pad}<a {idAttr} class=\"el\" {pos} href=\"{WebUtility.HtmlEncode(props.Href ?? "#")}\">{WebUtility.HtmlEncode(props.Text ?? "Link")}</a>",
            ElementType.Input => RenderInput(el, props, indent, relativePosition),
            ElementType.TextArea => RenderTextArea(el, props, indent, relativePosition),
            ElementType.Checkbox => RenderCheckbox(el, props, indent, relativePosition),
            ElementType.Form => RenderForm(el, props, all, pages, comments, indent),
            ElementType.Navbar => RenderNavbar(el, props, pages, indent),
            ElementType.Footer => $"{pad}<footer {idAttr} class=\"el footer-el\" {pos}>{WebUtility.HtmlEncode(props.Text ?? "Footer")}</footer>",
            ElementType.Gallery => RenderGallery(el, props, indent),
            ElementType.Slider => $"{pad}<div {idAttr} class=\"el slider-el\" {pos}><div class=\"slider-track\">{WebUtility.HtmlEncode(props.Text ?? "Слайдер")}</div></div>",
            ElementType.FAQ => RenderFaq(el, props, indent),
            ElementType.CommentsBlock => RenderCommentsBlock(el, props, comments, indent),
            ElementType.VideoEmbed => RenderVideoEmbed(el, props, indent, relativePosition),
            ElementType.VideoPlayer => RenderVideoPlayer(el, props, indent, relativePosition),
            ElementType.CommentsViewer => RenderCommentsViewer(el, props, comments, all, indent),
            _ => $"{pad}<div {idAttr} class=\"el\" {pos}>{WebUtility.HtmlEncode(props.Text ?? el.Type.ToString())}</div>"
        };
    }

    private string RenderContainer(PageElement el, ElementPropertiesDto props, List<PageElement> all,
        List<Page> pages, List<PageComment> comments, int indent)
    {
        var pad = new string(' ', indent);
        var flex = props.FlexDirection ?? "column";
        var gap = props.Gap ?? "8px";
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<div id=\"el-{el.Id}\" class=\"el container-box\" {Pos(el)} style=\"{ExportLayoutHelper.BuildInlinePositionStyle(el)};display:flex;flex-direction:{flex};gap:{gap};align-items:flex-start;\">");
        if (!string.IsNullOrEmpty(props.Text))
            sb.AppendLine($"{pad}  <span class=\"container-label\">{WebUtility.HtmlEncode(props.Text)}</span>");
        foreach (var child in all.Where(c => c.ParentElementId == el.Id).OrderBy(c => c.SortOrder))
            sb.AppendLine(RenderElement(child, all, pages, comments, indent + 2, true));
        sb.AppendLine($"{pad}</div>");
        return sb.ToString();
    }

    private static string RenderCard(PageElement el, ElementPropertiesDto props, List<Page> pages, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var onclick = GetButtonOnClick(el, pages);
        var clickAttr = string.IsNullOrEmpty(onclick) ? "" : $" onclick=\"{onclick}\"";
        var pos = relative ? RelativePos(el) : Pos(el);
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<article id=\"el-{el.Id}\" class=\"el card-el\" {pos}{clickAttr} role=\"button\" tabindex=\"0\">");
        if (!string.IsNullOrWhiteSpace(props.Src))
            sb.AppendLine($"{pad}  <img class=\"card-img\" src=\"{WebUtility.HtmlEncode(props.Src)}\" alt=\"{WebUtility.HtmlEncode(props.Text ?? "")}\" />");
        sb.AppendLine($"{pad}  <div class=\"card-body\">");
        sb.AppendLine($"{pad}    <h4 class=\"card-title\">{WebUtility.HtmlEncode(props.Text ?? "Карточка")}</h4>");
        sb.AppendLine($"{pad}    <p class=\"card-desc\">{WebUtility.HtmlEncode(props.Description ?? "")}</p>");
        sb.AppendLine($"{pad}  </div>");
        sb.AppendLine($"{pad}</article>");
        return sb.ToString();
    }

    private static string RenderInput(PageElement el, ElementPropertiesDto props, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var name = WebUtility.HtmlEncode(props.FieldName ?? $"field_{el.Id}");
        var formAttr = props.FormElementId.HasValue ? $" data-form-id=\"{props.FormElementId}\"" : "";
        var btnAttr = props.LinkedButtonId.HasValue ? $" data-linked-button=\"{props.LinkedButtonId}\"" : "";
        var pos = relative ? RelativePos(el) : Pos(el);
        return $"{pad}<input id=\"el-{el.Id}\" class=\"el form-field\" {pos} type=\"{props.InputType ?? "text"}\" name=\"{name}\" placeholder=\"{WebUtility.HtmlEncode(props.Placeholder ?? "")}\" {(props.Required ? "required" : "")}{formAttr}{btnAttr} />";
    }

    private static string RenderTextArea(PageElement el, ElementPropertiesDto props, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var name = WebUtility.HtmlEncode(props.FieldName ?? $"field_{el.Id}");
        var formAttr = props.FormElementId.HasValue ? $" data-form-id=\"{props.FormElementId}\"" : "";
        var pos = relative ? RelativePos(el) : Pos(el);
        return $"{pad}<textarea id=\"el-{el.Id}\" class=\"el form-field\" {pos} name=\"{name}\" placeholder=\"{WebUtility.HtmlEncode(props.Placeholder ?? "")}\" {formAttr}></textarea>";
    }

    private static string RenderCheckbox(PageElement el, ElementPropertiesDto props, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var variant = props.CheckboxVariant ?? "default";
        var name = WebUtility.HtmlEncode(props.FieldName ?? $"check_{el.Id}");
        var formAttr = props.FormElementId.HasValue ? $" data-form-id=\"{props.FormElementId}\"" : "";
        var inputType = variant == "radio" ? "radio" : "checkbox";
        var pos = relative ? RelativePos(el) : Pos(el);
        return $"{pad}<label id=\"el-{el.Id}\" class=\"el checkbox-el checkbox-{variant}\" {pos}{formAttr}>" +
               $"<input type=\"{inputType}\" name=\"{name}\" value=\"1\" {(props.Required ? "required" : "")} /> " +
               $"{WebUtility.HtmlEncode(props.Text ?? "")}</label>";
    }

    private string RenderForm(PageElement el, ElementPropertiesDto props, List<PageElement> all,
        List<Page> pages, List<PageComment> comments, int indent)
    {
        var pad = new string(' ', indent);
        var method = props.FormMethod ?? "post";
        var action = string.IsNullOrEmpty(props.FormActionUrl) ? "#" : WebUtility.HtmlEncode(props.FormActionUrl);
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<form id=\"el-{el.Id}\" class=\"el form-box\" {Pos(el)} method=\"{method}\" action=\"{action}\" data-form-id=\"{el.Id}\">");
        if (!string.IsNullOrEmpty(props.Text))
            sb.AppendLine($"{pad}  <div class=\"form-title\">{WebUtility.HtmlEncode(props.Text)}</div>");

        var fields = all.Where(e => e.ParentElementId == el.Id || e.FormElementIdFromProps(el.Id))
            .OrderBy(e => e.SortOrder).ToList();
        foreach (var field in fields)
            sb.AppendLine(RenderElement(field, all, pages, comments, indent + 2, true));

        sb.AppendLine($"{pad}</form>");
        return sb.ToString();
    }

    private static string RenderButton(PageElement el, ElementPropertiesDto props, List<Page> pages, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var onclick = GetButtonOnClick(el, pages);
        var clickAttr = string.IsNullOrEmpty(onclick) ? "" : $" onclick=\"{onclick}\"";
        var typeAttr = el.Action?.ActionType == ActionType.SubmitForm ? " type=\"submit\"" : " type=\"button\"";
        var formAttr = props.FormElementId.HasValue ? $" form=\"el-{props.FormElementId}\"" : "";
        var pos = relative ? RelativePos(el) : Pos(el);
        return $"{pad}<button id=\"el-{el.Id}\" class=\"el btn-primary\" {pos}{typeAttr}{formAttr}{clickAttr}>{WebUtility.HtmlEncode(props.Text ?? "Button")}</button>";
    }

    private static string? GetButtonOnClick(PageElement el, List<Page> pages)
    {
        if (el.Action == null) return null;
        return el.Action.ActionType switch
        {
            ActionType.NavigateToPage when el.Action.TargetPageId.HasValue =>
                $"navigateToPage('{GetPageFile(pages, el.Action.TargetPageId.Value)}')",
            ActionType.OpenUrl when !string.IsNullOrEmpty(el.Action.TargetUrl) =>
                $"window.open('{WebUtility.HtmlEncode(el.Action.TargetUrl)}','_blank')",
            ActionType.SubmitForm when el.Action.TargetElementId.HasValue =>
                $"submitForm_{el.Action.TargetElementId.Value}(event)",
            ActionType.CustomScript when !string.IsNullOrEmpty(el.Action.CustomJavaScript) =>
                $"runAction_{el.Action.Id}()",
            ActionType.OpenPopup => $"openPopup_{el.Action.Id}()",
            _ => null
        };
    }

    private static string RenderNavbar(PageElement el, ElementPropertiesDto props, List<Page> pages, int indent)
    {
        var pad = new string(' ', indent);
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<nav id=\"el-{el.Id}\" class=\"el navbar-el\" {Pos(el)}>");
        sb.AppendLine($"{pad}  <div class=\"navbar-logo\">");
        if (!string.IsNullOrWhiteSpace(props.LogoSrc))
            sb.AppendLine($"{pad}    <img src=\"{WebUtility.HtmlEncode(props.LogoSrc)}\" alt=\"{WebUtility.HtmlEncode(props.LogoText ?? "Logo")}\" class=\"navbar-logo-img\" />");
        else
            sb.AppendLine($"{pad}    <strong>{WebUtility.HtmlEncode(props.LogoText ?? "Logo")}</strong>");
        sb.AppendLine($"{pad}  </div>");
        sb.AppendLine($"{pad}  <div class=\"navbar-links\">");
        foreach (var item in props.NavItems ?? new List<NavItemDto>())
        {
            var href = item.TargetPageId.HasValue
                ? GetPageFile(pages, item.TargetPageId.Value)
                : WebUtility.HtmlEncode(item.Url ?? "#");
            sb.AppendLine($"{pad}    <a href=\"{href}\">{WebUtility.HtmlEncode(item.Text)}</a>");
        }
        sb.AppendLine($"{pad}  </div>");
        sb.AppendLine($"{pad}</nav>");
        return sb.ToString();
    }

    private static string RenderGallery(PageElement el, ElementPropertiesDto props, int indent)
    {
        var pad = new string(' ', indent);
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<div id=\"el-{el.Id}\" class=\"el gallery-el\" {Pos(el)}>");
        foreach (var src in props.GalleryImages ?? new List<string>())
            sb.AppendLine($"{pad}  <img src=\"{WebUtility.HtmlEncode(src)}\" alt=\"\" />");
        sb.AppendLine($"{pad}</div>");
        return sb.ToString();
    }

    private static string RenderFaq(PageElement el, ElementPropertiesDto props, int indent)
    {
        var pad = new string(' ', indent);
        var sb = new StringBuilder($"{pad}<div id=\"el-{el.Id}\" class=\"el faq-el\" {Pos(el)}>");
        foreach (var item in props.FaqItems ?? new List<FaqItemDto>())
            sb.AppendLine($"{pad}  <details><summary>{WebUtility.HtmlEncode(item.Question)}</summary><p>{WebUtility.HtmlEncode(item.Answer)}</p></details>");
        sb.AppendLine($"{pad}</div>");
        return sb.ToString();
    }

    private static string RenderVideoEmbed(PageElement el, ElementPropertiesDto props, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var pos = relative ? RelativePos(el) : Pos(el);
        var embed = VideoUrlHelper.ToEmbedUrl(props.VideoUrl);
        if (string.IsNullOrWhiteSpace(embed))
            return $"{pad}<div id=\"el-{el.Id}\" class=\"el video-embed-el\" {pos}><span>Видео не задано</span></div>";
        return $"{pad}<div id=\"el-{el.Id}\" class=\"el video-embed-el\" {pos}>" +
               $"<iframe src=\"{WebUtility.HtmlEncode(embed)}\" title=\"video\" allowfullscreen loading=\"lazy\"></iframe></div>";
    }

    private static string RenderVideoPlayer(PageElement el, ElementPropertiesDto props, int indent, bool relative)
    {
        var pad = new string(' ', indent);
        var pos = relative ? RelativePos(el) : Pos(el);
        var url = WebUtility.HtmlEncode(props.VideoUrl ?? "");
        var poster = string.IsNullOrWhiteSpace(props.VideoPosterUrl) ? "" : $" poster=\"{WebUtility.HtmlEncode(props.VideoPosterUrl)}\"";
        var controls = props.VideoControls ? " controls" : "";
        var autoplay = props.VideoAutoplay ? " autoplay muted" : "";
        var loop = props.VideoLoop ? " loop" : "";
        return $"{pad}<div id=\"el-{el.Id}\" class=\"el video-player-el\" {pos}>" +
               $"<video src=\"{url}\"{poster}{controls}{autoplay}{loop} playsinline></video></div>";
    }

    private static string RenderCommentsViewer(PageElement el, ElementPropertiesDto props,
        List<PageComment> comments, List<PageElement> pageElements, int indent)
    {
        var pad = new string(' ', indent);
        var max = props.MaxComments ?? 50;
        var sourceId = CommentsHelper.ResolveSourceElementId(el, props, pageElements);
        var blockComments = comments.Where(c => c.ElementId == sourceId).Take(max).ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<section id=\"el-{el.Id}\" class=\"el comments-viewer\" {Pos(el)} data-comments-viewer=\"{el.Id}\" data-comments-source=\"{sourceId}\">");
        sb.AppendLine($"{pad}  <h3 class=\"comments-title\">{WebUtility.HtmlEncode(props.Text ?? "Комментарии")}</h3>");
        sb.AppendLine($"{pad}  <div class=\"comments-list\">");
        foreach (var c in blockComments)
        {
            sb.AppendLine($"{pad}    <article class=\"comment-item\">");
            if (props.ShowCommentAuthor)
                sb.AppendLine($"{pad}      <strong>{WebUtility.HtmlEncode(c.AuthorName)}</strong>");
            sb.AppendLine($"{pad}      <p>{WebUtility.HtmlEncode(c.Text)}</p>");
            if (props.ShowCommentDate)
                sb.AppendLine($"{pad}      <time class=\"comment-date\">{c.CreatedAt.ToLocalTime():g}</time>");
            sb.AppendLine($"{pad}    </article>");
        }
        if (blockComments.Count == 0)
            sb.AppendLine($"{pad}    <p class=\"comments-empty\">Комментариев пока нет.</p>");
        sb.AppendLine($"{pad}  </div>");
        sb.AppendLine($"{pad}</section>");
        return sb.ToString();
    }

    private static string RenderCommentsBlock(PageElement el, ElementPropertiesDto props,
        List<PageComment> comments, int indent)
    {
        var pad = new string(' ', indent);
        var blockComments = comments.Where(c => c.ElementId == el.Id).ToList();
        var max = props.MaxComments ?? 100;
        var sb = new StringBuilder();
        sb.AppendLine($"{pad}<section id=\"el-{el.Id}\" class=\"el comments-block\" {Pos(el)} data-comments-block=\"{el.Id}\" data-max=\"{max}\">");
        sb.AppendLine($"{pad}  <h3 class=\"comments-title\">{WebUtility.HtmlEncode(props.Text ?? "Комментарии")}</h3>");
        sb.AppendLine($"{pad}  <div class=\"comments-list\" id=\"comments-list-{el.Id}\">");
        foreach (var c in blockComments)
        {
            sb.AppendLine($"{pad}    <article class=\"comment-item\" data-id=\"{c.Id}\">");
            sb.AppendLine($"{pad}      <strong>{WebUtility.HtmlEncode(c.AuthorName)}</strong>");
            sb.AppendLine($"{pad}      <p>{WebUtility.HtmlEncode(c.Text)}</p>");
            sb.AppendLine($"{pad}    </article>");
        }
        sb.AppendLine($"{pad}  </div>");
        if (props.AllowComments)
        {
            sb.AppendLine($"{pad}  <div class=\"comments-form\">");
            sb.AppendLine($"{pad}    <input type=\"text\" class=\"comment-author\" placeholder=\"Ваше имя\" />");
            sb.AppendLine($"{pad}    <textarea class=\"comment-text\" placeholder=\"Комментарий...\"></textarea>");
            sb.AppendLine($"{pad}    <button type=\"button\" class=\"btn-primary comment-submit\" data-block=\"{el.Id}\">Отправить</button>");
            sb.AppendLine($"{pad}  </div>");
        }
        sb.AppendLine($"{pad}</section>");
        return sb.ToString();
    }

    private static string GetPageFile(List<Page> pages, int pageId)
    {
        var p = pages.FirstOrDefault(x => x.Id == pageId);
        return p == null ? "index.html" : (p.IsHome ? "index.html" : $"{p.Slug}.html");
    }
}

internal static class FormElementExtensions
{
    public static bool FormElementIdFromProps(this PageElement e, int formId)
    {
        var props = ElementJsonHelper.DeserializeProperties(e.PropertiesJson);
        return props.FormElementId == formId;
    }
}
