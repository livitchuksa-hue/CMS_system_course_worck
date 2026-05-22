using CMSBuilder.Data;

using CMSBuilder.Helpers;

using CMSBuilder.Models;

using CMSBuilder.Models.Dto;

using CMSBuilder.Models.Enums;

using Microsoft.EntityFrameworkCore;



namespace CMSBuilder.Services;



public class ElementService

{

    public List<PageElement> LoadPageElements(int pageId) =>

        LoadAllPageElements(pageId).Where(e => e.ParentElementId == null).OrderBy(e => e.SortOrder).ToList();



    public List<PageElement> LoadAllPageElements(int pageId)

    {

        using var db = new AppDbContext();

        var list = db.PageElements

            .Include(e => e.Action)

            .Where(e => e.PageId == pageId)

            .OrderBy(e => e.SortOrder)

            .ToList();

        foreach (var el in list)
            ComplexElementService.HydrateProperties(el, db);

        return list;

    }



    public PageElement AddElement(int pageId, ElementType type, double x, double y, int? parentId = null)

    {

        using var db = new AppDbContext();

        var maxOrder = db.PageElements.Where(e => e.PageId == pageId && e.ParentElementId == parentId)

            .Select(e => (int?)e.SortOrder).Max() ?? -1;



        var props = CreateDefaultProperties(type);

        var style = CreateDefaultStyle(type);



        var (w, h) = GetDefaultSize(type);

        var element = new PageElement

        {

            PageId = pageId,

            ParentElementId = parentId,

            Type = type,

            SortOrder = maxOrder + 1,

            X = x,

            Y = y,

            Width = w,

            Height = h,

            PropertiesJson = ElementJsonHelper.SerializeProperties(props),

            StyleJson = ElementJsonHelper.SerializeStyle(style)

        };

        db.PageElements.Add(element);

        db.SaveChanges();

        ComplexElementService.EnsureCreated(db, element, props);

        db.SaveChanges();

        return element;

    }



    public void SaveElement(PageElement element)

    {

        using var db = new AppDbContext();

        var existing = db.PageElements.Find(element.Id);

        if (existing == null) return;

        existing.PropertiesJson = element.PropertiesJson;

        existing.StyleJson = element.StyleJson;

        existing.X = element.X;

        existing.Y = element.Y;

        existing.Width = element.Width;

        existing.Height = element.Height;

        existing.ActionId = element.ActionId;

        existing.SortOrder = element.SortOrder;

        existing.ParentElementId = element.ParentElementId;

        ComplexElementService.Persist(db, element);

        db.SaveChanges();

    }



    public void SaveAllElements(IEnumerable<PageElement> elements)

    {

        foreach (var el in elements)

            SaveElement(el);

    }



    public void DeleteElement(int elementId)

    {

        using var db = new AppDbContext();

        var el = db.PageElements.Find(elementId);

        if (el == null) return;



        var children = db.PageElements.Where(e => e.ParentElementId == elementId).ToList();

        foreach (var child in children)

        {

            child.ParentElementId = null;

            child.X += el.X;

            child.Y += el.Y;

        }



        ComplexElementService.DeleteForElement(db, el);

        db.PageComments.RemoveRange(db.PageComments.Where(c => c.ElementId == elementId));

        db.PageElements.Remove(el);

        db.SaveChanges();

    }



    public ElementAction CreateOrUpdateAction(int websiteId, ElementAction action)

    {

        using var db = new AppDbContext();

        if (action.Id == 0)

        {

            action.WebsiteId = websiteId;

            db.ElementActions.Add(action);

        }

        else

        {

            var existing = db.ElementActions.Find(action.Id);

            if (existing != null)

            {

                existing.Name = action.Name;

                existing.ActionType = action.ActionType;

                existing.TargetPageId = action.TargetPageId;

                existing.TargetElementId = action.TargetElementId;

                existing.TargetUrl = action.TargetUrl;

                existing.CustomJavaScript = action.CustomJavaScript;

                existing.PopupHtml = action.PopupHtml;

                action = existing;

            }

        }

        db.SaveChanges();

        return action;

    }



    public List<ElementAction> GetWebsiteActions(int websiteId)

    {

        using var db = new AppDbContext();

        return db.ElementActions.Where(a => a.WebsiteId == websiteId).ToList();

    }



    private static ElementPropertiesDto CreateDefaultProperties(ElementType type) => type switch

    {

        ElementType.Text => new ElementPropertiesDto { Text = "Новый текст" },

        ElementType.Header => new ElementPropertiesDto { Text = "Заголовок", Level = "h2" },

        ElementType.Image => new ElementPropertiesDto { Src = "https://via.placeholder.com/400x200", Alt = "image" },

        ElementType.Button => new ElementPropertiesDto { Text = "Кнопка" },

        ElementType.Card => new ElementPropertiesDto

        {

            Text = "Название карточки",

            Description = "Краткое описание",

            Src = "https://via.placeholder.com/280x160"

        },

        ElementType.Input => new ElementPropertiesDto { Placeholder = "Введите текст...", InputType = "text" },

        ElementType.Link => new ElementPropertiesDto { Text = "Ссылка", Href = "#" },

        ElementType.Navbar => new ElementPropertiesDto

        {

            LogoText = "Logo",

            NavItems = new List<NavItemDto>

            {

                new() { Text = "Главная" },

                new() { Text = "О нас" }

            }

        },

        ElementType.Gallery => new ElementPropertiesDto

        {

            GalleryImages = new List<string>

            {

                "https://via.placeholder.com/200",

                "https://via.placeholder.com/200",

                "https://via.placeholder.com/200"

            }

        },

        ElementType.Container => new ElementPropertiesDto { Text = "Контейнер", FlexDirection = "column", Gap = "8px" },

        ElementType.Form => new ElementPropertiesDto { Text = "Форма", FormMethod = "post" },

        ElementType.Checkbox => new ElementPropertiesDto { Text = "Согласен", CheckboxVariant = "default", FieldName = "agree" },

        ElementType.CommentsBlock => new ElementPropertiesDto { Text = "Комментарии", AllowComments = true, MaxComments = 100 },

        ElementType.FAQ => new ElementPropertiesDto

        {

            FaqItems = new List<FaqItemDto>

            {

                new() { Question = "Вопрос 1?", Answer = "Ответ 1." }

            }

        },

        ElementType.TextArea => new ElementPropertiesDto { Placeholder = "Введите сообщение...", FieldName = "message" },

        ElementType.Divider => new ElementPropertiesDto(),

        ElementType.Spacer => new ElementPropertiesDto(),

        ElementType.VideoEmbed => new ElementPropertiesDto
        {
            VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
        },

        ElementType.VideoPlayer => new ElementPropertiesDto
        {
            VideoUrl = "https://www.w3schools.com/html/mov_bbb.mp4",
            VideoControls = true
        },

        ElementType.CommentsViewer => new ElementPropertiesDto
        {
            Text = "Комментарии",
            MaxComments = 50,
            ShowCommentAuthor = true,
            ShowCommentDate = true,
            AllowComments = false
        },

        _ => new ElementPropertiesDto { Text = type.ToString() }

    };



    private static (double? w, double? h) GetDefaultSize(ElementType type) => type switch

    {

        ElementType.Image => (240d, 140d),

        ElementType.Button => (120d, 40d),

        ElementType.Card => (220d, 260d),

        ElementType.Input => (220d, 36d),

        ElementType.Navbar => (880d, 48d),

        ElementType.Gallery => (400d, 160d),

        ElementType.Container => (300d, 160d),

        ElementType.Form => (320d, 200d),

        ElementType.Checkbox => (200d, 32d),

        ElementType.CommentsBlock => (360d, 220d),

        ElementType.Header => (320d, null),

        ElementType.Text => (200d, null),

        ElementType.VideoEmbed => (360d, 200d),

        ElementType.VideoPlayer => (360d, 220d),

        ElementType.CommentsViewer => (360d, 240d),

        _ => (160d, 40d)

    };



    private static ElementStyleDto CreateDefaultStyle(ElementType type) => type switch

    {

        ElementType.Button => new ElementStyleDto

        {

            BackgroundColor = "#1B6B3A",

            Color = "#FFFFFF",

            Padding = "12px 24px",

            BorderRadius = "8px",

            FontSize = "14px"

        },

        ElementType.Card => new ElementStyleDto

        {

            BackgroundColor = "#FFFFFF",

            BorderColor = "#D8E0DC",

            BorderWidth = "1px",

            BorderRadius = "10px",

            Padding = "0"

        },

        ElementType.Container => new ElementStyleDto

        {

            BackgroundColor = "#EEF2F0",

            BorderColor = "#1B6B3A",

            BorderWidth = "1px",

            BorderRadius = "8px",

            Padding = "12px"

        },

        ElementType.Header => new ElementStyleDto { FontSize = "28px", FontWeight = "600", Color = "#1A2420" },

        ElementType.Text => new ElementStyleDto { FontSize = "16px", Color = "#1A2420" },

        ElementType.VideoEmbed => new ElementStyleDto { BorderRadius = "8px" },

        ElementType.VideoPlayer => new ElementStyleDto { BorderRadius = "8px", BackgroundColor = "#000000" },

        ElementType.CommentsViewer => new ElementStyleDto
        {
            BackgroundColor = "#FFFFFF",
            BorderColor = "#D8E0DC",
            BorderWidth = "1px",
            BorderRadius = "8px",
            Padding = "12px"
        },

        _ => new ElementStyleDto()

    };

}


