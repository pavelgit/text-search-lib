using System;

namespace TextSearchLib.Core
{
    public class DefaultFileSystemWatcher: IDisposable
    {

        public void AddFile(string absolutePath)
        {
            var watcher = new SingleFileWatcher(absolutePath);
            watcher.FileChanged += (sender, s) => Console.WriteLine($"File changed: {s}"); 
        }
        
        public void Watch(string path)
        {
            
        }
        
        public void Dispose()
        {
            
        }
        
    }
}