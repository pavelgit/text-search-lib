using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TextSearchLib.Core.Watchers;

namespace TextSearchLib.Core
{
    /// <summary>
    /// Provides functionality for searching text across multiple files with real-time file system monitoring.
    /// </summary>
    public class TextFinder : IDisposable
    {
        private readonly FileIndexer _fileIndexer;
        private readonly CombinedFileSystemWatcher _fileSystemWatcher;
        private bool _disposed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TextFinder"/> class.
        /// </summary>
        /// <param name="wordSplitter">Optional function to split text into words. If not provided, uses default word splitting logic.</param>
        /// <param name="logger">Optional logger for verbose output.</param>
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

        /// <summary>
        /// Adds a single file to the search index and starts monitoring it for changes.
        /// </summary>
        /// <param name="filePath">The path to the file to add.</param>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        public void AddFile(string filePath)
        {
            var absolutePath = Path.GetFullPath(filePath);
            _fileSystemWatcher.AddFile(absolutePath);
            _fileIndexer.AddTextToIndex(File.ReadAllText(absolutePath), absolutePath);
        }

        /// <summary>
        /// Adds all text files from a directory to the search index and starts monitoring the directory for changes.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to add.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
        public void AddDirectory(string directoryPath)
        {
            var absolutePath = Path.GetFullPath(directoryPath);
            _fileSystemWatcher.AddDirectory(absolutePath);
        }

        /// <summary>
        /// Searches for files containing the specified word.
        /// </summary>
        /// <param name="word">The word to search for.</param>
        /// <returns>An enumerable of file paths containing the specified word.</returns>
        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            return _fileIndexer.FindFilesContainingWord(word);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="TextFinder"/>.
        /// </summary>
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