using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxFileSystemWatcher
{
    public class ObservableFileSystemWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private IObservable<FileSystemEvent> _allEvents { get; }
        private readonly Subject<FileSystemEvent> _events = new Subject<FileSystemEvent>();
        private readonly Subject<FileSystemEvent> _pollResults = new Subject<FileSystemEvent>();

        public IObservable<FileSystemEventArgs> Changed { get; }
        public IObservable<RenamedEventArgs> Renamed { get; }
        public IObservable<FileSystemEventArgs> Deleted { get; }
        public IObservable<Exception> Errors { get; }
        public IObservable<FileSystemEventArgs> Created { get; }

        public IObservable<FileSystemEvent> Events => _events;

        public ObservableFileSystemWatcher(Action<FileSystemWatcher> configure)
        {
            _watcher = new FileSystemWatcher();
            configure(_watcher);

            Changed = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _watcher.Changed += h, h => _watcher.Changed -= h)
                .Select(x => x.EventArgs);

            Renamed = Observable
                .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(h => _watcher.Renamed += h, h => _watcher.Renamed -= h)
                .Select(x => x.EventArgs);

            Deleted = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _watcher.Deleted += h, h => _watcher.Deleted -= h)
                .Select(x => x.EventArgs);

            Created = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _watcher.Created += h, h => _watcher.Created -= h)
                .Select(x => x.EventArgs);

            Errors = Observable
                .FromEventPattern<ErrorEventHandler, ErrorEventArgs>(h => _watcher.Error += h, h => _watcher.Error -= h)
                .Select(x => x.EventArgs.GetException());

            var changed = Changed.Select(c => c.ConvertTo());
            var deleted = Deleted.Select(c => c.ConvertTo());
            var renames = Renamed.Select(r => r.ConvertTo());
            var creates = Created.Select(c => c.ConvertTo());

            _allEvents = changed
                .Merge(deleted)
                .Merge(renames)
                .Merge(creates)
                .Merge(_pollResults);

            _allEvents.Subscribe(_events.OnNext);
            Errors.Subscribe(_events.OnError);
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        public void Dispose()
        {
            _events.OnCompleted();
            _watcher?.Dispose();
        }

        public void PollExisting()
        {
            foreach (var existingFile in Directory.GetFiles(_watcher.Path, _watcher.Filter, _watcher.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                _pollResults.OnNext(new FileSystemEvent(WatcherChangeTypes.Created, existingFile, Path.GetFileName(existingFile)));
            }
        }
    }
}
