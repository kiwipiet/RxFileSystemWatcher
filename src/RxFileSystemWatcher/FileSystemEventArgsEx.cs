using System.IO;

namespace RxFileSystemWatcher
{
    public static class FileSystemEventArgsEx
    {
        public static FileSystemEvent ConvertTo(this FileSystemEventArgs This)
        {
            return new FileSystemEvent(This.ChangeType, This.FullPath, This.Name);
        }
    }
}