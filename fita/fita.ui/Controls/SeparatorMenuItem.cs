using System.Windows;
using System.Windows.Controls;

namespace fita.ui.Controls;

public class SeparatorMenuItem : Control
{
    static SeparatorMenuItem() 
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(SeparatorMenuItem), new FrameworkPropertyMetadata(typeof(SeparatorMenuItem)));
}