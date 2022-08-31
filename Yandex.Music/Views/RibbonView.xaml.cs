using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yandex.Music.Core;
using Yandex.Music.ViewModels;

namespace Yandex.Music.Views;
/// <summary>
/// Логика взаимодействия для SearchView.xaml
/// </summary>
public partial class RibbonView : UserControl
{

    public RibbonView() {
        InitializeComponent();
        ((RibbonViewModel)DataContext).PageChanged += (s, e) => ScrollToStartPosition();
    }

    private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        if (e.Handled)
            return;
        e.Handled = true;
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
        eventArg.RoutedEvent = MouseWheelEvent;
        eventArg.Source = sender;
        var parent = ((Control)sender).Parent as UIElement;
        parent?.RaiseEvent(eventArg);
    }

    private void ScrollToStartPosition() {
        var selectedEntity = Ribbon.SelectedValue as EntityHandler;
        // Не возвращать в начало, если список пуст или выбран заголовок
        if (Ribbon.Items.Count == 0 || (selectedEntity?.IsCaption ?? false))
            return;
        Ribbon.ScrollIntoView(Ribbon.Items[0], Ribbon.Columns[0]);
    }
}


