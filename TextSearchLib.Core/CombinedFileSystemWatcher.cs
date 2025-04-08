using System;
using System.Collections.Generic;
using System.IO;

namespace TextSearchLib.Core
{
    public class CombinedFileSystemWatcher : IDisposable
    {
        private readonly Dictionary<string, SingleFileWatcher> _fileWatchers = new Dictionary<string, SingleFileWatcher>();
        private bool _disposed;
        
        public event EventHandler<string> FileChanged;
        
        public void AddFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
                
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);
                
            var absolutePath = Path.GetFullPath(filePath);
            
            // Don't add duplicate watchers
            if (_fileWatchers.ContainsKey(absolutePath))
                return;
                
            var watcher = new SingleFileWatcher(absolutePath);
            watcher.FileChanged += OnFileChanged;
            _fileWatchers.Add(absolutePath, watcher);
        }
        
        private void OnFileChanged(object sender, string filePath)
        {
            FileChanged?.Invoke(this, filePath);
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;
                
            foreach (var watcher in _fileWatchers.Values)
            {
                watcher.FileChanged -= OnFileChanged;
                watcher.Dispose();
            }
            
            _fileWatchers.Clear();
            _disposed = true;
        }
    }
}