using System;
using System.IO;

namespace TextSearchLib.Core.Watchers
{
    /// <summary>
    /// Monitors a single file for changes, deletions, and moves.
    /// </summary>
    public class SingleFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private bool _disposed;
        
        /// <summary>
        /// Occurs when the monitored file is changed.
        /// </summary>
        public event EventHandler<string> FileChanged;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleFileWatcher"/> class.
        /// </summary>
        /// <param name="absoluteFilePath">The absolute path of the file to monitor.</param>
        public SingleFileWatcher(string absoluteFilePath)
        {
            if (string.IsNullOrEmpty(absoluteFilePath))
            {
                throw new ArgumentNullException(nameof(absoluteFilePath));
            }

            if (!File.Exists(absoluteFilePath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {absoluteFilePath}");
            }

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(absoluteFilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(absoluteFilePath),
                EnableRaisingEvents = true
            };
            
            _watcher.Changed += OnFileChanged;
        }
        
        /// <summary>
        /// Handles file system change events.
        /// </summary>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            FileChanged?.Invoke(this, e.FullPath);
        }
        
        /// <summary>
        /// Releases all resources used by the <see cref="SingleFileWatcher"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _watcher.Changed -= OnFileChanged;
            _watcher.Dispose();
            _disposed = true;
        }
    }
}