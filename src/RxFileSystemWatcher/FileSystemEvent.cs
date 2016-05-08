using System.IO;

namespace RxFileSystemWatcher
{
    public class FileSystemEvent
    {
        public WatcherChangeTypes ChangeType { get; }
        public string FullPath { get; }
        public string Name { get; }

        public FileSystemEvent(WatcherChangeTypes changeType, string fullPath, string name)
        {
            ChangeType = changeType;
            FullPath = fullPath;
            Name = name;
        }
    }
}