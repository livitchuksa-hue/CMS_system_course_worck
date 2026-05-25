using CMSBuilder.Models;
using CMSBuilder.Models.Dto;
using CMSBuilder.Models.Enums;

namespace CMSBuilder.Helpers;

public static class CommentsHelper
{
    /// <summary>
    /// Id элемента CommentsBlock, чьи комментарии нужно показывать.
    /// </summary>
    public static int ResolveSourceElementId(PageElement element, ElementPropertiesDto props,
        IEnumerable<PageElement> pageElements)
    {
        if (element.Type == ElementType.CommentsBlock)
            return element.Id;

        if (props.CommentsSourceElementId is int configured &&
            pageElements.Any(e => e.Id == configured && e.Type == ElementType.CommentsBlock))
            return configured;

        var blocks = pageElements.Where(e => e.Type == ElementType.CommentsBlock).ToList();
        if (blocks.Count == 1)
            return blocks[0].Id;

        return element.Id;
    }
}
