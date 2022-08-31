using System.Windows;

namespace Yandex.Music.Views
{
    /// <summary>
    /// Логика взаимодействия для DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
           var test = ContentView.DataContext;
        }
    }
}
