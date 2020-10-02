using System.Windows;
using CVCapturePanel.ViewModel;

namespace CVCapturePanel.View
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel(Close);
            DataContext = vm;
        }
    }
}