using System.Windows;
using System.Windows.Controls;

namespace Yandex.Music.Views;
/// <summary>
/// Логика взаимодействия для AuthorizationView.xaml
/// </summary>
public partial class AuthorizationView : UserControl
{
    public AuthorizationView() {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) {
        if (DataContext != null)
            ((dynamic)DataContext).Password = ((PasswordBox)sender).Password;
    }
}
