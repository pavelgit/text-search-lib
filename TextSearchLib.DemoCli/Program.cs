using System.Text.RegularExpressions;
using TextSearchLib.Core;

Console.WriteLine("TextSearchLib Demo");
Console.WriteLine("=================");
Console.WriteLine();

// Create a text searcher instance
var textFinder = new TextFinder(text => Regex.Split(text, @"\W+"));

textFinder.AddFile("/Users/checkito120/private/net-experiments/files/cats.txt");
textFinder.AddFile("/Users/checkito120/private/net-experiments/files/dogs.txt");
textFinder.AddDirectory("/Users/checkito120/private/net-experiments/files/nested");

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