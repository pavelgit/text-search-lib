using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TextSearchLib.Core
{
    public class FileIndexer
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _index 
            = new ConcurrentDictionary<string, HashSet<string>>();
        
        private readonly Func<string, IEnumerable<string>> _wordSplitter;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        
        private readonly bool _ignoreCase;

        public FileIndexer(Func<string, IEnumerable<string>> wordSplitter, bool ignoreCase = true)
        {
            _wordSplitter = wordSplitter ?? throw new ArgumentNullException(nameof(wordSplitter));
            _ignoreCase = ignoreCase;
        }

        public void AddTextToIndex(string text, string relatedFilePath)
        {
            var words = _wordSplitter(text).Distinct();
            if (_ignoreCase)
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

        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            if (_ignoreCase)
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