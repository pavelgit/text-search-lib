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
            var absolutePath = Path.GetFullPath(filePath);
            
            _fileIndexer.AddTextToIndex(File.ReadAllText(absolutePath), absolutePath);
            _fileSystemWatcher.AddFile(absolutePath);
            
            _fileSystemWatcher.FileChanged += (sender, changedFileAbsolutePath) =>
            {
                _fileIndexer.AddTextToIndex(File.ReadAllText(changedFileAbsolutePath), changedFileAbsolutePath);
            };
        }

        public void AddDirectory(string directoryPath)
        {
            var absolutePath = Path.GetFullPath(directoryPath);
            
            _fileSystemWatcher.AddDirectory(absolutePath);
            
            _fileSystemWatcher.FileChanged += (sender, changedFileAbsolutePath) =>
            {
                _fileIndexer.AddTextToIndex(File.ReadAllText(changedFileAbsolutePath), changedFileAbsolutePath);
            };
        }
        
        public IEnumerable<string> FindFilesContainingWord(string word)
        {
            return _fileIndexer.FindFilesContainingWord(word);
        }
    }

}