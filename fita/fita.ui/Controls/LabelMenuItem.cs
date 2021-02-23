using System.Windows;
using System.Windows.Controls;

namespace fita.ui.Controls
{
    public class LabelMenuItem : Control
    {
        public static readonly DependencyProperty LeftCaptionProperty = DependencyProperty.Register(
            "LeftCaption",
            typeof(string),
            typeof(LabelMenuItem),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RightCaptionProperty = DependencyProperty.Register(
            "RightCaption",
            typeof(string),
            typeof(LabelMenuItem),
            new PropertyMetadata(string.Empty));

        static LabelMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelMenuItem), new FrameworkPropertyMetadata(typeof(LabelMenuItem)));
        }

        public string LeftCaption
        {
            get => (string)this.GetValue(LeftCaptionProperty);
            set => this.SetValue(LeftCaptionProperty, value);
        }

        public string RightCaption
        {
            get => (string)this.GetValue(RightCaptionProperty);
            set => this.SetValue(RightCaptionProperty, value);
        }
    }
}
