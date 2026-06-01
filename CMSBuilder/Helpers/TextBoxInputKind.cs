namespace CMSBuilder.Helpers;

/// <summary>
/// Тип ввода для TextBox (поля ссылок — <see cref="Url"/>, без ограничений).
/// </summary>
public enum TextBoxInputKind
{
  None,
  Text,
  Numeric,
  NumericPx,
  HexColor,
  Slug,
  Integer,
  Multiline,
  Url
}
