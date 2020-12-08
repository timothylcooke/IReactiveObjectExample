using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace IReactiveObjectExample
{
    /// <summary>
    /// Interaction logic for MyUserControl.xaml
    /// </summary>
    public partial class MyUserControl : ReactiveUserControl<MyViewModel>
    {
        public MyUserControl()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm != null)
                .BindTo(this, x => x.DataContext);

            Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                    x => AddNameButton.Click += x,
                    x => AddNameButton.Click -= x,
                    RxApp.MainThreadScheduler
                )
                .Subscribe(_ => ViewModel.AddName());
        }
    }
}
