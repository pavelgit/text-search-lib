using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TextSearchLib.Core.Watchers;

namespace TextSearchLib.Core
{
    public class TextFinder : IDisposable
    {
        private readonly FileIndexer _fileIndexer;
        private readonly CombinedFileSystemWatcher _fileSystemWatcher;
        private bool _disposed;
        
        public TextFinder(Func<string, IEnumerable<string>> wordSplitter = null, ILogger<TextFinder> logger = null)
        {
            if (wordSplitter == null)
            {
                wordSplitter = text => Regex.Split(text, @"\W+");
            }
            
            _fileIndexer = new FileIndexer(wordSplitter);
            _fileSystemWatcher = new CombinedFileSystemWatcher();
            
            _fileSystemWatcher.FileChanged += (sender, changedFileAbsolutePath) =>
            {
                logger?.LogDebug("File changed: {FilePath}", changedFileAbsolutePath);
                _fileIndexer.RemoveFileFromIndex(changedFileAbsolutePath);
                _fileIndexer.AddTextToIndex(File.ReadAllText(changedFileAbsolutePath), changedFileAbsolutePath);
            };
            
            _fileSystemWatcher.FileDetected += (sender, changedFileAbsolutePath) =>
            {
                logger?.LogDebug("File detected: {FilePath}", changedFileAbsolutePath);
                _fileIndexer.AddTextToIndex(File.ReadAllText(changedFileAbsolutePath), changedFileAbsolutePath);
            };
            
            _fileSystemWatcher.FileGone += (sender, changedFileAbsolutePath) =>
            {
                logger?.LogDebug("File gone: {FilePath}", changedFileAbsolutePath);
               _fileIndexer.RemoveFileFromIndex(changedFileAbsolutePath);
            };
        }

        public void AddFile(string filePath)
        {
            var absolutePath = Path.GetFullPath(filePath);
            _fileSystemWatcher.AddFile(absolutePath);
            _fileIndexer.AddTextToIndex(File.ReadAllText(absolutePath), absolutePath);
        }

        public void AddDirectory(string directoryPath)
        {
            var absolutePath = Path.GetFullPath(directoryPath);
            _fileSystemWatcher.AddDirectory(absolutePath);
        }

        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            return _fileIndexer.FindFilesContainingWord(word);
        }

        public void Dispose() 
        {
            if (_disposed)
            {
                return;
            }

            _fileSystemWatcher.Dispose();
            
            _disposed = true;
        }
    }
}