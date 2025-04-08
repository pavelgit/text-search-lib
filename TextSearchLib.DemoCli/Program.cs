using System.Text.RegularExpressions;
using TextSearchLib.Core;

Console.WriteLine("TextSearchLib Demo");
Console.WriteLine("=================");
Console.WriteLine();

// Create a text searcher instance
var textFinder = new TextFinder(text => Regex.Split(text, @"\W+"));

var path = "/Users/checkito120/private/net-experiments/text-search-lib/file.txt";

textFinder.AddFile(path);

Console.WriteLine("Enter words to search (press Ctrl+C to exit):");

while (true)
{
    Console.Write("> ");
    var searchText = Console.ReadLine();
    if (!string.IsNullOrEmpty(searchText))
    {
        var results = textFinder.FindFilesContainingWord(searchText);
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
    }
}