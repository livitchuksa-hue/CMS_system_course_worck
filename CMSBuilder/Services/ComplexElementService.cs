using CMSBuilder.Data;
using CMSBuilder.Helpers;
using CMSBuilder.Models;
using CMSBuilder.Models.ComplexElements;
using CMSBuilder.Models.Dto;
using CMSBuilder.Models.Enums;

namespace CMSBuilder.Services;

public static class ComplexElementService
{
    public static bool HasDedicatedTable(ElementType type) =>
        type is ElementType.Card or ElementType.VideoPlayer or ElementType.CommentsViewer;

    public static void EnsureCreated(AppDbContext db, PageElement element, ElementPropertiesDto props)
    {
        switch (element.Type)
        {
            case ElementType.Card:
                if (db.CardElementData.Find(element.Id) != null) return;
                db.CardElementData.Add(new CardElementData
                {
                    PageElementId = element.Id,
                    Title = props.Text ?? "Карточка",
                    Description = props.Description ?? string.Empty,
                    ImageUrl = props.Src
                });
                break;
            case ElementType.VideoPlayer:
                if (db.VideoPlayerElementData.Find(element.Id) != null) return;
                db.VideoPlayerElementData.Add(new VideoPlayerElementData
                {
                    PageElementId = element.Id,
                    VideoUrl = props.VideoUrl ?? string.Empty,
                    PosterUrl = props.VideoPosterUrl,
                    ShowControls = props.VideoControls,
                    Autoplay = props.VideoAutoplay,
                    Loop = props.VideoLoop
                });
                break;
            case ElementType.CommentsViewer:
                if (db.CommentsViewerElementData.Find(element.Id) != null) return;
                db.CommentsViewerElementData.Add(new CommentsViewerElementData
                {
                    PageElementId = element.Id,
                    Title = props.Text ?? "Комментарии",
                    MaxVisible = props.MaxComments ?? 50,
                    ShowAuthor = props.ShowCommentAuthor,
                    ShowDate = props.ShowCommentDate
                });
                break;
        }
    }

    public static void Persist(AppDbContext db, PageElement element)
    {
        var props = ElementJsonHelper.DeserializeProperties(element.PropertiesJson);
        switch (element.Type)
        {
            case ElementType.Card:
                var card = db.CardElementData.Find(element.Id);
                if (card == null)
                {
                    EnsureCreated(db, element, props);
                    card = db.CardElementData.Find(element.Id);
                }
                if (card != null)
                {
                    card.Title = props.Text ?? string.Empty;
                    card.Description = props.Description ?? string.Empty;
                    card.ImageUrl = props.Src;
                }
                break;
            case ElementType.VideoPlayer:
                var player = db.VideoPlayerElementData.Find(element.Id);
                if (player == null)
                {
                    EnsureCreated(db, element, props);
                    player = db.VideoPlayerElementData.Find(element.Id);
                }
                if (player != null)
                {
                    player.VideoUrl = props.VideoUrl ?? string.Empty;
                    player.PosterUrl = props.VideoPosterUrl;
                    player.ShowControls = props.VideoControls;
                    player.Autoplay = props.VideoAutoplay;
                    player.Loop = props.VideoLoop;
                }
                break;
            case ElementType.CommentsViewer:
                var viewer = db.CommentsViewerElementData.Find(element.Id);
                if (viewer == null)
                {
                    EnsureCreated(db, element, props);
                    viewer = db.CommentsViewerElementData.Find(element.Id);
                }
                if (viewer != null)
                {
                    viewer.Title = props.Text ?? "Комментарии";
                    viewer.MaxVisible = props.MaxComments ?? 50;
                    viewer.ShowAuthor = props.ShowCommentAuthor;
                    viewer.ShowDate = props.ShowCommentDate;
                }
                break;
        }
    }

    public static void HydrateProperties(PageElement element, AppDbContext db)
    {
        if (!HasDedicatedTable(element.Type)) return;

        var props = ElementJsonHelper.DeserializeProperties(element.PropertiesJson);
        switch (element.Type)
        {
            case ElementType.Card:
                var card = db.CardElementData.Find(element.Id);
                if (card == null)
                {
                    EnsureCreated(db, element, props);
                    card = db.CardElementData.Find(element.Id);
                }
                if (card != null)
                {
                    props.Text = card.Title;
                    props.Description = card.Description;
                    props.Src = card.ImageUrl;
                }
                break;
            case ElementType.VideoPlayer:
                var player = db.VideoPlayerElementData.Find(element.Id);
                if (player == null)
                {
                    EnsureCreated(db, element, props);
                    player = db.VideoPlayerElementData.Find(element.Id);
                }
                if (player != null)
                {
                    props.VideoUrl = player.VideoUrl;
                    props.VideoPosterUrl = player.PosterUrl;
                    props.VideoControls = player.ShowControls;
                    props.VideoAutoplay = player.Autoplay;
                    props.VideoLoop = player.Loop;
                }
                break;
            case ElementType.CommentsViewer:
                var viewer = db.CommentsViewerElementData.Find(element.Id);
                if (viewer == null)
                {
                    EnsureCreated(db, element, props);
                    viewer = db.CommentsViewerElementData.Find(element.Id);
                }
                if (viewer != null)
                {
                    props.Text = viewer.Title;
                    props.MaxComments = viewer.MaxVisible;
                    props.ShowCommentAuthor = viewer.ShowAuthor;
                    props.ShowCommentDate = viewer.ShowDate;
                    props.AllowComments = false;
                }
                break;
        }
        element.PropertiesJson = ElementJsonHelper.SerializeProperties(props);
    }

    public static void HydrateAll(IEnumerable<PageElement> elements)
    {
        using var db = new AppDbContext();
        foreach (var el in elements.Where(e => HasDedicatedTable(e.Type)))
            HydrateProperties(el, db);
    }

    public static void DeleteForElement(AppDbContext db, PageElement element)
    {
        switch (element.Type)
        {
            case ElementType.Card:
                var card = db.CardElementData.Find(element.Id);
                if (card != null) db.CardElementData.Remove(card);
                break;
            case ElementType.VideoPlayer:
                var player = db.VideoPlayerElementData.Find(element.Id);
                if (player != null) db.VideoPlayerElementData.Remove(player);
                break;
            case ElementType.CommentsViewer:
                var viewer = db.CommentsViewerElementData.Find(element.Id);
                if (viewer != null) db.CommentsViewerElementData.Remove(viewer);
                break;
        }
    }
}
