using System;
using System.IO;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Shouldly;

namespace RxFileSystemWatcher.Tests
{
    public class FileSystemEventWatcherTests : FileIntegrationTestsBase
    {
        private Action<FileSystemWatcher> _config;
        private CancellationTokenSource _cts;
        private const string _watchedFilename = "FileSystemEventWatcherTests.txt";

        public override void BeforeEachTest()
        {
            base.BeforeEachTest();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(500);

            _config = w =>
            {
                w.Path = TempPath;
                w.Filter = _watchedFilename;
                w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            };
        }
        public async Task NewFile_NoExistingFile_StreamsDropped()
        {
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var firstEvent = watcher.Events.FirstAsync().ToTask(_cts.Token);
                watcher.Start();

                var monitoredFile = Path.Combine(TempPath, _watchedFilename);
                File.WriteAllText(monitoredFile, "foo");

                var fileSystemEvent = await firstEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Created);
            }
        }

        public async Task FileRenamed_NoExistingFile_StreamsDropped()
        {
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var firstEvent = watcher.Events.FirstAsync().ToTask(_cts.Token);
                var otherFile = Path.Combine(TempPath, "Other.Txt");
                File.WriteAllText(otherFile, "foo");
                watcher.Start();

                var monitoredFile = Path.Combine(TempPath, _watchedFilename);
                File.Move(otherFile, monitoredFile);

                var fileSystemEvent = await firstEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Renamed);
            }
        }

        public async Task Overwrite_ExistingFile_StreamsDropped()
        {
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var firstEvent = watcher.Events.FirstAsync().ToTask(_cts.Token);
                var monitoredFile = Path.Combine(TempPath, _watchedFilename);
                File.WriteAllText(monitoredFile, "foo");
                watcher.Start();

                File.WriteAllText(monitoredFile, "bar");

                var fileSystemEvent = await firstEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Changed);
            }
        }

        public async Task PollExisting_FileBeforeStart_StreamsDropped()
        {
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var firstEvent = watcher.Events.FirstAsync().ToTask(_cts.Token);
                var monitoredFile = Path.Combine(TempPath, _watchedFilename);
                File.WriteAllText(monitoredFile, "foo");

                watcher.PollExisting();

                var fileSystemEvent = await firstEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Created);
            }
        }

        public async Task PollExisting_SecondTime_StreamsSecondTime()
        {
            var monitoredFile = Path.Combine(TempPath, _watchedFilename);
            File.WriteAllText(monitoredFile, "foo");
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var secondEvent = watcher.Events.Skip(1).FirstAsync().ToTask(_cts.Token);

                watcher.PollExisting();
                watcher.PollExisting();

                var fileSystemEvent = await secondEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Created);
            }
        }
        public async Task DeleteExistingFile_StreamsDropped()
        {
            var monitoredFile = Path.Combine(TempPath, _watchedFilename);
            File.WriteAllText(monitoredFile, "foo");
            using (var watcher = new ObservableFileSystemWatcher(_config))
            {
                var firstEvent = watcher.Events.FirstAsync().ToTask(_cts.Token);
                watcher.Start();

                File.Delete(monitoredFile);

                var fileSystemEvent = await firstEvent;
                fileSystemEvent.Name.ShouldBe(_watchedFilename);
                fileSystemEvent.FullPath.ShouldBe(monitoredFile);
                fileSystemEvent.ChangeType.ShouldBe(WatcherChangeTypes.Deleted);
            }
        }
    }
}