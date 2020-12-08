using System.Windows;

namespace IReactiveObjectExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReactiveObject.ViewModel = new MyViewModel(new ReactiveObjectFilter('A'), new ReactiveObjectFilter('B'), new ReactiveObjectFilter('C'), new ReactiveObjectFilter('D'));
            IReactiveObject.ViewModel = new MyViewModel(new IReactiveObjectFilter('A'), new IReactiveObjectFilter('B'), new IReactiveObjectFilter('C'), new IReactiveObjectFilter('D'));
        }
    }
}
