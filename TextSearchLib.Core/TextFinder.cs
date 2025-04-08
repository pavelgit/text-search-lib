using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextSearchLib.Core
{
    public class TextFinder
    {
        private readonly FileIndexer _fileIndexer;
        private CombinedFileSystemWatcher _fileSystemWatcher = new CombinedFileSystemWatcher();
        
        public TextFinder(Func<string, IEnumerable<string>> wordSplitter)
        {
            _fileIndexer = new FileIndexer(wordSplitter);
        }

        public void AddFile(string filePath)
        {
            _fileIndexer.AddTextToIndex(File.ReadAllText(filePath), filePath);
            _fileSystemWatcher.AddFile(filePath);
            
            _fileSystemWatcher.FileChanged += (sender, s) =>
            {
                _fileIndexer.AddTextToIndex(File.ReadAllText(filePath), filePath);
            };
        }
        
        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            return _fileIndexer.FindFilesContainingWord(word);
        }
    }

}