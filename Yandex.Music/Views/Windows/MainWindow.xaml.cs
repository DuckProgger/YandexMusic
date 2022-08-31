using System.Windows;

namespace Yandex.Music.Views;

public partial class MainWindow : Window
{
    public MainWindow() {
        InitializeComponent();
    }

    private void Close(object sender, RoutedEventArgs e) {
        Close();
    }
}
