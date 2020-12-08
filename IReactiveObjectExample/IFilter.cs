using System.ComponentModel;
using ReactiveUI;

namespace IReactiveObjectExample
{
    public interface IFilter : INotifyPropertyChanged
    {
        char FirstChar { get; }
        bool ShouldFilter { get; }
    }

    // Note that the below classes are almost exactly identical. They both call this.RaiseAndSetIfChanged to update the _shouldFilter field. The only
    // difference is that the IReactiveObject must implement the interface here, whereas the ReactiveObject implements the interface in the base class.

    public class IReactiveObjectFilter : IReactiveObject, IFilter
    {
        private bool _shouldFilter;

        public IReactiveObjectFilter(char firstChar)
        {
            FirstChar = firstChar;
        }

        public char FirstChar { get; }
        public bool ShouldFilter
        {
            get => _shouldFilter;
            set => this.RaiseAndSetIfChanged(ref _shouldFilter, value);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public void RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            PropertyChanging?.Invoke(this, args);
        }
        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }

    public class ReactiveObjectFilter : ReactiveObject, IFilter
    {
        private bool _shouldFilter;

        public ReactiveObjectFilter(char firstChar)
        {
            FirstChar = firstChar;
        }

        public char FirstChar { get; }
        public bool ShouldFilter
        {
            get => _shouldFilter;
            set => this.RaiseAndSetIfChanged(ref _shouldFilter, value);
        }
    }
}
