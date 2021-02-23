using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace fita.ui.Controls
{
    public class ClickableMenuItem : RadioButton
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(ClickableMenuItem),
            new PropertyMetadata(null));

        public static readonly DependencyProperty LeftCaptionProperty = DependencyProperty.Register(
            "LeftCaption",
            typeof(string),
            typeof(ClickableMenuItem),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RightCaptionProperty = DependencyProperty.Register(
            "RightCaption",
            typeof(string),
            typeof(ClickableMenuItem),
            new PropertyMetadata(string.Empty));

        static ClickableMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClickableMenuItem), new FrameworkPropertyMetadata(typeof(ClickableMenuItem)));
        }

        public ImageSource Icon
        {
            get => (ImageSource)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
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
