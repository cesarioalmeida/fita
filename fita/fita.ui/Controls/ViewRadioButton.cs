using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace fita.ui.Controls
{
    public class ViewRadioButton : RadioButton
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(ViewRadioButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            "Caption",
            typeof(string),
            typeof(ViewRadioButton),
            new PropertyMetadata(string.Empty));

        static ViewRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewRadioButton), new FrameworkPropertyMetadata(typeof(ViewRadioButton)));
        }

        public ImageSource Icon
        {
            get => (ImageSource)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public string Caption
        {
            get => (string)this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }
    }
}
