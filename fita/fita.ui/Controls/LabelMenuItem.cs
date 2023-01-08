using System.Windows;
using System.Windows.Controls;

namespace fita.ui.Controls;

public class LabelMenuItem : Control
{
    public static readonly DependencyProperty LeftCaptionProperty = DependencyProperty.Register(
        nameof(LeftCaption),
        typeof(string),
        typeof(LabelMenuItem),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty RightCaptionProperty = DependencyProperty.Register(
        nameof(RightCaption),
        typeof(string),
        typeof(LabelMenuItem),
        new PropertyMetadata(string.Empty));

    static LabelMenuItem() 
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelMenuItem), new FrameworkPropertyMetadata(typeof(LabelMenuItem)));

    public string LeftCaption
    {
        get => (string)GetValue(LeftCaptionProperty);
        set => SetValue(LeftCaptionProperty, value);
    }

    public string RightCaption
    {
        get => (string)GetValue(RightCaptionProperty);
        set => SetValue(RightCaptionProperty, value);
    }
}