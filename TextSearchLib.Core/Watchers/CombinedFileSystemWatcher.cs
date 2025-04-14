using System;
using System.Collections.Generic;
using System.IO;
using TextSearchLib.Core.Watchers;

namespace TextSearchLib.Core
{
    public class CombinedFileSystemWatcher : IDisposable
    {
        private readonly Dictionary<string, SingleFileWatcher> _fileWatchers = new Dictionary<string, SingleFileWatcher>();
        private readonly Dictionary<string, RecursiveDirectoryWatcher> _directoryWatchers = new Dictionary<string, RecursiveDirectoryWatcher>();
        private bool _disposed;
        
        public event EventHandler<string> FileChanged;
        public event EventHandler<string> FileDetected;
        public event EventHandler<string> FileGone;
        
        public void AddFile(string absoluteFilePath)
        {
            if (_fileWatchers.ContainsKey(absoluteFilePath))
            {
                return;
            }

            var watcher = new SingleFileWatcher(absoluteFilePath);
            watcher.FileChanged += OnFileChanged;
            _fileWatchers.Add(absoluteFilePath, watcher);
        }
        
        public void AddDirectory(string absoluteDirectoryPath)
        {
            if (_directoryWatchers.ContainsKey(absoluteDirectoryPath))
            {
                return;
            }

            var watcher = new RecursiveDirectoryWatcher(absoluteDirectoryPath);
            watcher.FileChanged += OnFileChanged;
            watcher.FileDetected += OnFileDetected;
            watcher.FileGone += OnFileGone;
            _directoryWatchers.Add(absoluteDirectoryPath, watcher);
     
            watcher.ProcessDirectoryDetection(absoluteDirectoryPath);
        }
        
        private void OnFileChanged(object sender, string filePath)
        {
            FileChanged?.Invoke(this, filePath);
        }
        
        private void OnFileDetected(object sender, string filePath)
        {
            FileDetected?.Invoke(this, filePath);
        }
        
        private void OnFileGone(object sender, string filePath)
        {
            FileGone?.Invoke(this, filePath);
        }
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var watcher in _fileWatchers.Values)
            {
                watcher.FileChanged -= OnFileChanged;
                watcher.Dispose();
            }  
            
            foreach (var watcher in _directoryWatchers.Values)
            {
                watcher.FileChanged -= OnFileChanged;
                watcher.FileDetected -= OnFileChanged;
                watcher.FileGone -= OnFileChanged;
                watcher.Dispose();
            }
            
            _fileWatchers.Clear();
            _disposed = true;
        }
    }
}