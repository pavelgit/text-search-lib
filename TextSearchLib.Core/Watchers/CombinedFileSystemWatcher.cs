using System;
using System.Collections.Generic;

namespace TextSearchLib.Core.Watchers
{
    /// <summary>
    /// Manages multiple file and directory watchers, providing a unified interface for file system monitoring.
    /// </summary>
    public class CombinedFileSystemWatcher : IDisposable
    {
        private readonly Dictionary<string, SingleFileWatcher> _fileWatchers = new Dictionary<string, SingleFileWatcher>();
        private readonly Dictionary<string, RecursiveDirectoryWatcher> _directoryWatchers = new Dictionary<string, RecursiveDirectoryWatcher>();
        private bool _disposed;
        
        /// <summary>
        /// Occurs when a monitored file is changed.
        /// </summary>
        public event EventHandler<string> FileChanged;
        
        /// <summary>
        /// Occurs when a new file is detected in a monitored directory.
        /// </summary>
        public event EventHandler<string> FileDetected;
        
        /// <summary>
        /// Occurs when a monitored file is deleted or moved.
        /// </summary>
        public event EventHandler<string> FileGone;
        
        /// <summary>
        /// Adds a file to be monitored for changes.
        /// </summary>
        /// <param name="absoluteFilePath">The absolute path of the file to monitor.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the watcher has been disposed.</exception>
        public void AddFile(string absoluteFilePath)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CombinedFileSystemWatcher));
            }

            if (_fileWatchers.ContainsKey(absoluteFilePath))
            {
                return;
            }

            var watcher = new SingleFileWatcher(absoluteFilePath);
            watcher.FileChanged += OnFileChanged;
            _fileWatchers.Add(absoluteFilePath, watcher);
        }
        
        /// <summary>
        /// Adds a directory to be monitored for changes, including all its subdirectories.
        /// </summary>
        /// <param name="absoluteDirectoryPath">The absolute path of the directory to monitor.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the watcher has been disposed.</exception>
        public void AddDirectory(string absoluteDirectoryPath)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CombinedFileSystemWatcher));
            }

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
        
        /// <summary>
        /// Handles the FileChanged event from individual watchers.
        /// </summary>
        private void OnFileChanged(object sender, string filePath)
        {
            FileChanged?.Invoke(this, filePath);
        }
        
        /// <summary>
        /// Handles the FileDetected event from directory watchers.
        /// </summary>
        private void OnFileDetected(object sender, string filePath)
        {
            FileDetected?.Invoke(this, filePath);
        }
        
        /// <summary>
        /// Handles the FileGone event from directory watchers.
        /// </summary>
        private void OnFileGone(object sender, string filePath)
        {
            FileGone?.Invoke(this, filePath);
        }
        
        /// <summary>
        /// Releases all resources used by the <see cref="CombinedFileSystemWatcher"/>.
        /// </summary>
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