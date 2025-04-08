using System;
using System.IO;

namespace TextSearchLib.Core
{
    public class SingleFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;
        public event EventHandler<string> FileChanged;
        
        public SingleFileWatcher(string absoluteFilePath)
        {
            if (string.IsNullOrEmpty(absoluteFilePath))
                throw new ArgumentNullException(nameof(absoluteFilePath));

            if (!File.Exists(absoluteFilePath))
                throw new DirectoryNotFoundException($"Directory not found: {absoluteFilePath}");
            
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(absoluteFilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(absoluteFilePath),
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