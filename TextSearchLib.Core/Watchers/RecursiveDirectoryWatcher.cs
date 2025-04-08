using System;
using System.Collections.Generic;
using System.IO;

namespace TextSearchLib.Core
{
    public class RecursiveDirectoryWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly HashSet<string> _detectedFiles = new HashSet<string>();
        
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
        }

        public void ProcessDirectoryDetection(string directoryPath)
        {
            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                ProcessSingleFileDetection(filePath);
            }
        }

        private void ProcessSingleFileDetection(string absoluteFilePath)
        {
            if (_detectedFiles.Add(absoluteFilePath))
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
                ProcessDirectoryDetection(e.FullPath);
            else if (File.Exists(e.FullPath))
                ProcessSingleFileDetection(e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var fileRemoved = ProcessSingleFileDeletion(e.FullPath);
            if (!fileRemoved)
                ProcessDirectoryDeletion(e.FullPath);
        }
        
        private bool ProcessSingleFileDeletion(string absoluteFilePath)
        {
            if (_detectedFiles.Remove(absoluteFilePath))
            {
                FileGone?.Invoke(this, absoluteFilePath);
                return true;
            }

            return false;
        }

        private void ProcessDirectoryDeletion(string absoluteDirectoryPath)
        {
            foreach (var filePath in _detectedFiles)
            {
                if (filePath.StartsWith(absoluteDirectoryPath))
                {
                    ProcessSingleFileDeletion(filePath);
                }
            }
        }

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
