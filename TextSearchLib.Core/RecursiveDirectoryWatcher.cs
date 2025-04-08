using System;
using System.IO;

namespace TextSearchLib.Core
{
    public class RecursiveDirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;
        public event EventHandler<string> FileChanged;
        
        public RecursiveDirectoryWatcher(string absolutePath)
        {
            _watcher = new FileSystemWatcher(absolutePath)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(absolutePath),
                EnableRaisingEvents = true
            };
            
            _watcher.Changed += OnFileChanged;
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            FileChanged?.Invoke(this, e.FullPath);
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;
                
            _watcher.Changed -= OnFileChanged;
            _watcher.Dispose();
            _disposed = true;
        }
    }
}