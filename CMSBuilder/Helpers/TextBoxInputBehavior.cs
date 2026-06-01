using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CMSBuilder.Services;

namespace CMSBuilder.Helpers;

public static class TextBoxInputBehavior
{
  public static readonly DependencyProperty KindProperty =
      DependencyProperty.RegisterAttached(
          "Kind",
          typeof(TextBoxInputKind),
          typeof(TextBoxInputBehavior),
          new PropertyMetadata(TextBoxInputKind.None, OnKindChanged));

  public static TextBoxInputKind GetKind(DependencyObject obj) => (TextBoxInputKind)obj.GetValue(KindProperty);

  public static void SetKind(DependencyObject obj, TextBoxInputKind value) => obj.SetValue(KindProperty, value);

  private static readonly HashSet<TextBox> Attached = new();

  private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is not TextBox box) return;

    if (e.OldValue is TextBoxInputKind oldKind && oldKind != TextBoxInputKind.None)
      Detach(box);

    if (GetKind(box) == TextBoxInputKind.None) return;

    if (GetKind(box) == TextBoxInputKind.HexColor)
      box.MaxLength = 7;

    box.PreviewTextInput += OnPreviewTextInput;
    DataObject.AddPastingHandler(box, OnPaste);
    box.LostFocus += OnLostFocus;
    Attached.Add(box);
  }

  private static void Detach(TextBox box)
  {
    box.PreviewTextInput -= OnPreviewTextInput;
    DataObject.RemovePastingHandler(box, OnPaste);
    box.LostFocus -= OnLostFocus;
    Attached.Remove(box);
  }

  private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
  {
    if (sender is not TextBox box) return;
    e.Handled = !IsTextAllowed(box, e.Text);
  }

  private static void OnPaste(object sender, DataObjectPastingEventArgs e)
  {
    if (sender is not TextBox box) return;
    if (GetKind(box) == TextBoxInputKind.Url || GetKind(box) == TextBoxInputKind.Multiline) return;

    if (e.DataObject.GetDataPresent(typeof(string)))
    {
      var paste = e.DataObject.GetData(typeof(string)) as string ?? string.Empty;
      if (!IsTextAllowed(box, paste))
        e.CancelCommand();
    }
  }

  private static void OnLostFocus(object sender, RoutedEventArgs e)
  {
    if (sender is not TextBox box) return;
    var kind = GetKind(box);

    if (kind == TextBoxInputKind.HexColor)
    {
      var normalized = PxValueHelper.NormalizeHexColor(box.Text);
      if (box.Text != normalized)
        box.Text = normalized;
    }
    else if (kind == TextBoxInputKind.Slug)
    {
      box.Text = WebsiteService.Slugify(box.Text);
    }
  }

  private static bool IsTextAllowed(TextBox box, string text)
  {
    if (string.IsNullOrEmpty(text)) return true;

    var kind = GetKind(box);
    if (kind is TextBoxInputKind.Url or TextBoxInputKind.Multiline or TextBoxInputKind.Text or TextBoxInputKind.None)
      return true;

    var current = box.Text ?? string.Empty;
    var selectionStart = box.SelectionStart;
    var selectionLength = box.SelectionLength;
    var proposed = current.Remove(selectionStart, selectionLength).Insert(selectionStart, text);

    return kind switch
    {
      TextBoxInputKind.Numeric or TextBoxInputKind.NumericPx or TextBoxInputKind.Integer =>
          proposed.All(char.IsDigit),

      TextBoxInputKind.HexColor => IsHexProposedValid(proposed),

      TextBoxInputKind.Slug => proposed.All(c => char.IsLetterOrDigit(c) || c == '-'),

      _ => true
    };
  }

  private static bool IsHexProposedValid(string proposed)
  {
    if (proposed.Length > 7) return false;
    for (var i = 0; i < proposed.Length; i++)
    {
      var c = proposed[i];
      if (i == 0) { if (c != '#') return false; }
      else if (!Uri.IsHexDigit(c)) return false;
    }
    return true;
  }
}
