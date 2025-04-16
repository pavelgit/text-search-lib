using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TextSearchLib.Core
{
    /// <summary>
    /// Maintains an index of words and their associated files for efficient text search operations.
    /// </summary>
    public class FileIndexer
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _index 
            = new ConcurrentDictionary<string, HashSet<string>>();
        
        private readonly Func<string, IEnumerable<string>> _wordSplitter;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        
        private readonly bool _caseSensitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIndexer"/> class.
        /// </summary>
        /// <param name="caseSensitive">Whether to perform case-insensitive search. Default is true.</param>
        /// <param name="wordSplitter">Function to split text into words.</param>
        /// <exception cref="ArgumentNullException">Thrown when wordSplitter is null.</exception>
        public FileIndexer(bool caseSensitive, Func<string, IEnumerable<string>> wordSplitter)
        {
            _caseSensitive = caseSensitive;
            _wordSplitter = wordSplitter ?? throw new ArgumentNullException(nameof(wordSplitter));
        }

        /// <summary>
        /// Adds text content to the index, associating it with a specific file.
        /// </summary>
        /// <param name="text">The text content to index.</param>
        /// <param name="relatedFilePath">The path of the file containing the text.</param>
        public void AddTextToIndex(string text, string relatedFilePath)
        {
            var words = _wordSplitter(text).Distinct();
            if (!_caseSensitive)
            {
                words = words.Select(word => word.ToLowerInvariant());
            }

            _lock.EnterWriteLock();
            try
            {
                foreach (var word in words)
                {
                    if (!_index.TryGetValue(word, out var fileSet))
                    {
                        fileSet = new HashSet<string>();
                        _index[word] = fileSet;
                    }
                    fileSet.Add(relatedFilePath);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Removes a file from the index.
        /// </summary>
        /// <param name="absoluteFilePath">The absolute path of the file to remove.</param>
        public void RemoveFileFromIndex(string absoluteFilePath)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var wordEntry in _index)
                {
                    wordEntry.Value.Remove(absoluteFilePath);
                }

                CleanupWordsWithNoFiles();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes words from the index that no longer have any associated files.
        /// </summary>
        private void CleanupWordsWithNoFiles()
        {
            var emptyWords = 
                _index.Where(entry => entry.Value.Count == 0)
                    .Select(entry => entry.Key)
                    .ToList();

            foreach (var word in emptyWords)
            {
                _index.TryRemove(word, out _);
            }
        }

        /// <summary>
        /// Searches for files containing the specified word.
        /// </summary>
        /// <param name="word">The word to search for.</param>
        /// <returns>An enumerable of file paths containing the specified word.</returns>
        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            if (!_caseSensitive)
            {
                word = word.ToLowerInvariant();
            }
            _lock.EnterReadLock();
            try
            {
                if (_index.TryGetValue(word, out var files))
                {
                    return files.ToList();
                }
                return Enumerable.Empty<string>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}