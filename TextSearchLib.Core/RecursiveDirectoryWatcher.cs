using System;
using System.IO;

namespace TextSearchLib.Core
{
    public class RecursiveDirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;
        public event EventHandler<string> FileChanged;
        
        public RecursiveDirectoryWatcher(string absoluteDirectoryPath)
        {
            if (string.IsNullOrEmpty(absoluteDirectoryPath))
                throw new ArgumentNullException(nameof(absoluteDirectoryPath));
                
            if (!Directory.Exists(absoluteDirectoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {absoluteDirectoryPath}");
            
            _watcher = new FileSystemWatcher(absoluteDirectoryPath)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            
            _watcher.Changed += OnFileEvent;
            _watcher.Created += OnFileEvent;
        }
        
        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            // Only notify for files, not directories
            if (File.Exists(e.FullPath))
            {
                FileChanged?.Invoke(this, e.FullPath);
            }
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;
                
            if (_watcher != null)
            {
                _watcher.Changed -= OnFileEvent;
                _watcher.Created -= OnFileEvent;
                _watcher.Dispose();
            }
            
            _disposed = true;
        }
    }
}