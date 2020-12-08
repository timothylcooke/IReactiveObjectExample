using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace IReactiveObjectExample
{
    public class MyViewModel : ReactiveObject, IDisposable
    {
        private readonly Random _random = new Random();

        private readonly string[] _names =
        {
            "Abigail", "Bridget", "Caitlin", "Denise",
            "Aaron", "Brady", "Chris", "Doug",
        };

        private readonly SourceList<string> _logSource;
        private ReadOnlyObservableCollection<string> _log;

        private ReadOnlyObservableCollection<IFilter> _filters;

        private readonly SourceList<string> _namesSource;
        private ReadOnlyObservableCollection<string> _originalNames, _filteredNames;

        private readonly CompositeDisposable _disposables;

        public MyViewModel(params IFilter[] filters)
        {
            _disposables = new CompositeDisposable();

            _logSource = new SourceList<string>().DisposeWith(_disposables);
            _namesSource = new SourceList<string>().DisposeWith(_disposables);

            var filtersSource = new SourceList<IFilter>().DisposeWith(_disposables);
            filtersSource.AddRange(filters);

            var names = _namesSource
                    .Connect()
                    .Publish()
                    .RefCount();

            var filtersChanged = filtersSource
                    .Connect()
                    .AutoRefresh(x => x.ShouldFilter)
                    .Publish()
                    .RefCount();
                
            foreach (var filter in filters)
            {
                filter.WhenAnyValue(x => x.ShouldFilter)
                    .Select(_ => $"Filter '{filter.FirstChar}': WhenAnyValue(x => x.{nameof(IFilter.ShouldFilter)}) fired.")
                    .Subscribe(AddLogItem)
                    .DisposeWith(_disposables);

                Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        x => filter.PropertyChanged += x,
                        x => filter.PropertyChanged -= x,
                        RxApp.MainThreadScheduler
                    )
                    .Select(x => $"Filter '{filter.FirstChar}': PropertyChanged(\"{x.EventArgs.PropertyName}\") fired.")
                    .Subscribe(AddLogItem)
                    .DisposeWith(_disposables);
            }

            _logSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _log)
                .Subscribe()
                .DisposeWith(_disposables);

            filtersSource
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filters)
                .Subscribe();

            names
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _originalNames)
                .Subscribe()
                .DisposeWith(_disposables);

            filtersChanged
                .Filter(x => x.ShouldFilter)
                .Transform(x => x.FirstChar)
                .Bind(out var currentFilters)
                .Subscribe()
                .DisposeWith(_disposables);

            var needToRequery =
                filtersChanged
                .Select(_ => $"filtersSource.Connect().AutoRefresh(x => x.{nameof(IFilter.ShouldFilter)}) fired.")
                .Merge(names.Select(_ => $"_namesSource.Connect() fired."));

            Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    x => ((INotifyCollectionChanged)currentFilters).CollectionChanged += x,
                    x => ((INotifyCollectionChanged)currentFilters).CollectionChanged -= x
                ).Select(x => $"currentFilters.CollectionChanged fired.")
                .Subscribe(AddLogItem)
                .DisposeWith(_disposables);

            needToRequery
                .Select(x => $"{x} Refreshing filtered names.")
                .Subscribe(AddLogItem)
                .DisposeWith(_disposables);

            names
                .Filter(needToRequery.Select(_ => new Func<string, bool>(x => !currentFilters.Contains(x[0]))))
                .Sort(SortExpressionComparer<string>.Ascending(x => x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filteredNames)
                .Subscribe()
                .DisposeWith(_disposables);
        }

        public void AddName()
        {
            var name = _names[_random.Next(_names.Length)];
            AddLogItem($"Add \"{name}\"");
            _namesSource.Add(name);
        }
        private void AddLogItem(string message)
        {
            _logSource.Add($"Thread {Thread.CurrentThread.ManagedThreadId}: {message}");
        }

        public ReadOnlyObservableCollection<string> Log => _log;
        public ReadOnlyObservableCollection<IFilter> Filters => _filters;
        public ReadOnlyObservableCollection<string> OriginalNames => _originalNames;
        public ReadOnlyObservableCollection<string> FilteredNames => _filteredNames;

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}