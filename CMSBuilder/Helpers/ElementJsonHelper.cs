using CMSBuilder.Models.Dto;
using Newtonsoft.Json;

namespace CMSBuilder.Helpers;

public static class ElementJsonHelper
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public static string SerializeProperties(ElementPropertiesDto dto) =>
        JsonConvert.SerializeObject(dto, Settings);

    public static ElementPropertiesDto DeserializeProperties(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new ElementPropertiesDto();
        return JsonConvert.DeserializeObject<ElementPropertiesDto>(json, Settings) ?? new ElementPropertiesDto();
    }

    public static string SerializeStyle(ElementStyleDto dto) =>
        JsonConvert.SerializeObject(dto, Settings);

    public static ElementStyleDto DeserializeStyle(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new ElementStyleDto();
        return JsonConvert.DeserializeObject<ElementStyleDto>(json, Settings) ?? new ElementStyleDto();
    }
}
