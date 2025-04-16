using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TextSearchLib.Core;

namespace TextSearchLib.Core.Tests
{
    public class FileIndexerTests
    {
        private FileIndexer _indexer;
        private readonly string _testFilePath1 = "test1.txt";
        private readonly string _testFilePath2 = "test2.txt";

        [SetUp]
        public void Setup()
        {
            _indexer = new FileIndexer(false, text => text.Split(' '));
        }

        [Test]
        public void AddTextToIndex_WithValidInput_AddsWordsToIndex()
        {
            // Arrange
            var text = "hello world";
            var filePath = _testFilePath1;

            // Act
            _indexer.AddTextToIndex(text, filePath);

            // Assert
            var helloFiles = _indexer.FindFilesContainingWord("hello").ToList();
            var worldFiles = _indexer.FindFilesContainingWord("world").ToList();

            Assert.That(helloFiles, Has.Count.EqualTo(1));
            Assert.That(worldFiles, Has.Count.EqualTo(1));
            Assert.AreEqual(filePath, helloFiles[0]);
            Assert.AreEqual(filePath, worldFiles[0]);
        }

        [Test]
        public void AddTextToIndex_WithEmptyText_DoesNotAddWords()
        {
            // Arrange
            var text = "";
            var filePath = _testFilePath1;

            // Act
            _indexer.AddTextToIndex(text, filePath);

            // Assert
            var result = _indexer.FindFilesContainingWord("anyword");
            Assert.IsEmpty(result as IEnumerable<string>);
        }

        [Test]
        public void AddTextToIndex_WithMultipleFiles_IndexesAllFiles()
        {
            // Arrange
            var text1 = "hello world";
            var text2 = "hello universe";

            // Act
            _indexer.AddTextToIndex(text1, _testFilePath1);
            _indexer.AddTextToIndex(text2, _testFilePath2);

            // Assert
            var helloFiles = _indexer.FindFilesContainingWord("hello").ToList();
            var worldFiles = _indexer.FindFilesContainingWord("world").ToList();
            var universeFiles = _indexer.FindFilesContainingWord("universe").ToList();

            Assert.That(helloFiles, Has.Count.EqualTo(2));
            Assert.That(worldFiles, Has.Count.EqualTo(1));
            Assert.That(universeFiles, Has.Count.EqualTo(1));
            Assert.That(helloFiles, Does.Contain(_testFilePath1));
            Assert.That(helloFiles, Does.Contain(_testFilePath2));
        }

        [Test]
        public void RemoveFileFromIndex_RemovesAllWordsForFile()
        {
            // Arrange
            _indexer.AddTextToIndex("hello world", _testFilePath1);
            _indexer.AddTextToIndex("hello universe", _testFilePath2);

            // Act
            _indexer.RemoveFileFromIndex(_testFilePath1);

            // Assert
            var helloFiles = _indexer.FindFilesContainingWord("hello").ToList();
            var worldFiles = _indexer.FindFilesContainingWord("world").ToList();

            Assert.That(helloFiles, Has.Count.EqualTo(1));
            Assert.AreEqual(_testFilePath2, helloFiles[0]);
            Assert.IsEmpty(worldFiles as IEnumerable<string>);
        }

        [Test]
        public void FindFilesContainingWord_WithNonExistentWord_ReturnsEmpty()
        {
            // Arrange
            _indexer.AddTextToIndex("hello world", _testFilePath1);

            // Act
            var result = _indexer.FindFilesContainingWord("nonexistent");

            // Assert
            Assert.IsEmpty(result as IEnumerable<string>);
        }
    }
}
