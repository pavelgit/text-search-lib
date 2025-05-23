using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSearchLib.Core.Watchers
{
    /// <summary>
    /// Monitors a directory and all its subdirectories for file changes, creations, and deletions.
    /// </summary>
    public class RecursiveDirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly HashSet<string> _detectedFilePaths = new HashSet<string>();
        
        private bool _disposed;

        /// <summary>
        /// Occurs when a file in the monitored directory is changed.
        /// </summary>
        public event EventHandler<string> FileChanged;
        
        /// <summary>
        /// Occurs when a new file is detected in the monitored directory.
        /// </summary>
        public event EventHandler<string> FileDetected;
        
        /// <summary>
        /// Occurs when a file in the monitored directory is deleted or moved.
        /// </summary>
        public event EventHandler<string> FileGone;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RecursiveDirectoryWatcher"/> class.
        /// </summary>
        /// <param name="absoluteDirectoryPath">The absolute path of the directory to monitor.</param>
        public RecursiveDirectoryWatcher(string absoluteDirectoryPath)
        {
            if (string.IsNullOrEmpty(absoluteDirectoryPath))
            {
                throw new ArgumentNullException(nameof(absoluteDirectoryPath));
            }

            if (!Directory.Exists(absoluteDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {absoluteDirectoryPath}");
            }

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
        }
        
        private bool AddDetectedFilePath(string absoluteFilePath)
        {
            lock (_detectedFilePaths)
            {
                return _detectedFilePaths.Add(absoluteFilePath);
            }
        }
        
        private bool RemoveDetectedFilePath(string absoluteFilePath)
        {
            lock (_detectedFilePaths)
            {
                return _detectedFilePaths.Remove(absoluteFilePath);
            }
        }
        
        private string[] GetDetectedFilePathsArray()
        {
            lock (_detectedFilePaths)
            {
                return _detectedFilePaths.ToArray();
            }
        }

        /// <summary>
        /// Processes the initial directory detection to find existing files.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to process.</param>
        public void ProcessDirectoryDetection(string directoryPath)
        {
            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                ProcessSingleFileDetection(filePath);
            }
        }

        private void ProcessSingleFileDetection(string absoluteFilePath)
        {
            if (AddDetectedFilePath(absoluteFilePath))
            {
                FileDetected?.Invoke(this, absoluteFilePath);
            }
        }

        /// <summary>
        /// Handles file change events.
        /// </summary>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                FileChanged?.Invoke(this, e.FullPath);
            }
        }

        /// <summary>
        /// Handles file creation events.
        /// </summary>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                ProcessDirectoryDetection(e.FullPath);
            }
            else if (File.Exists(e.FullPath))
            {
                ProcessSingleFileDetection(e.FullPath);
            }
        }

        /// <summary>
        /// Handles file deletion events.
        /// </summary>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var filePathWasInDetectedPaths = ProcessSingleFileDeletion(e.FullPath);
            if (!filePathWasInDetectedPaths)
            {
                // If the file path was not found in the _detectedFilePaths, it might be a directory
                ProcessDirectoryDeletion(e.FullPath);
            }
        }
        
        private bool ProcessSingleFileDeletion(string absoluteFilePath)
        {
            if (RemoveDetectedFilePath(absoluteFilePath))
            {
                FileGone?.Invoke(this, absoluteFilePath);
                return true;
            }

            return false;
        }

        private void ProcessDirectoryDeletion(string absoluteDirectoryPath)
        {
            foreach (var filePath in GetDetectedFilePathsArray())
            {
                if (filePath.StartsWith(absoluteDirectoryPath))
                {
                    ProcessSingleFileDeletion(filePath);
                }
            }
        }

        /// <summary>
        /// Handles file rename events.
        /// </summary>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                ProcessDirectoryDeletion(e.OldFullPath);
                ProcessDirectoryDetection(e.FullPath);
            } 
            else if (File.Exists(e.FullPath))
            {
                ProcessSingleFileDeletion(e.OldFullPath);
                ProcessSingleFileDetection(e.FullPath);
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="RecursiveDirectoryWatcher"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;

            _watcher.Dispose();
            _disposed = true;
        }
    }
}
