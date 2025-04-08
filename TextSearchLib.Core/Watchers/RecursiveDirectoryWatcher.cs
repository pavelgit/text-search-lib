using System;
using System.IO;

namespace TextSearchLib.Core
{
    public class RecursiveDirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;

        public event EventHandler<string> FileDetected;
        public event EventHandler<string> FileChanged;
        public event EventHandler<string> FileGone;

        public RecursiveDirectoryWatcher(string absoluteDirectoryPath)
        {
            if (string.IsNullOrEmpty(absoluteDirectoryPath))
                throw new ArgumentNullException(nameof(absoluteDirectoryPath));
            if (!Directory.Exists(absoluteDirectoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {absoluteDirectoryPath}");
            
            _watcher = new FileSystemWatcher(absoluteDirectoryPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            
            DetectFiles(absoluteDirectoryPath);
        }

        void DetectFiles(string directoryPath)
        {
            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                OnFileDetected(filePath);
            }
        }

        private void OnFileDetected(string absoluteFilePath)
        { 
            FileDetected?.Invoke(this, absoluteFilePath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
                FileChanged?.Invoke(this, e.FullPath);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath)) 
                DetectFiles(e.FullPath);
            else if (File.Exists(e.FullPath))
                FileDetected?.Invoke(this, e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            // File or directory deletion is detected, but we can't tell which from FileSystemEventArgs alone.
            FileGone?.Invoke(this, e.FullPath); // You may choose to rename this to ItemDeleted and treat generically
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
            if (Directory.Exists(e.FullPath))
                DirectoryRenamed?.Invoke(this, (e.OldFullPath, e.FullPath));
            else if (File.Exists(e.FullPath))
                FileRenamed?.Invoke(this, (e.OldFullPath, e.FullPath));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;

            _watcher.Dispose();
            _disposed = true;
        }
    }
}
