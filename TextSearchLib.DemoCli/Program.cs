using System.Text.RegularExpressions;
using TextSearchLib.Core;

namespace TextSearchLib.DemoCli;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🔍 Welcome to TextSearch Console");
        Console.WriteLine("Type 'help' for commands.");

        var textFinder = new TextFinder(text => Regex.Split(text, @"\W+"));

        while (true)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) 
                continue;

            var parts = input.Split(' ', 2);
            var command = parts[0].ToLowerInvariant();
            var argument = parts.Length > 1 ? parts[1] : null;

            try
            {
                switch (command)
                {
                    case "addfile":
                        if (string.IsNullOrWhiteSpace(argument) || !File.Exists(argument))
                        {
                            Console.WriteLine("❌ Invalid file path.");
                            break;
                        }

                        textFinder.AddFile(argument);
                        Console.WriteLine("✅ File added.");
                        break;

                    case "adddir":
                        if (string.IsNullOrWhiteSpace(argument) || !Directory.Exists(argument))
                        {
                            Console.WriteLine("❌ Invalid directory path.");
                            break;
                        }

                        textFinder.AddDirectory(argument);
                        Console.WriteLine("✅ Directory added.");
                        break;

                    case "find":
                        if (string.IsNullOrWhiteSpace(argument))
                        {
                            Console.WriteLine("❌ Enter a word to search.");
                            break;
                        }

                        var results = textFinder.FindFilesContainingWord(argument).ToList();
                        if (results.Count == 0)
                        {
                            Console.WriteLine("🔍 No files found.");
                        }
                        else
                        {
                            Console.WriteLine("📄 Files containing the word:");
                            foreach (var file in results)
                                Console.WriteLine($" - {file}");
                        }

                        break;

                    case "help":
                        PrintHelp();
                        break;

                    case "exit":
                        Console.WriteLine("👋 Exiting...");
                        return;

                    default:
                        Console.WriteLine("❓ Unknown command. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error: {ex.Message}");
            }
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"
Available commands:
  addfile <path>      Add a single text file to the index.
  adddir <path>       Add all text files from directory (recursively).
  find <word>       Search for files containing a word.
  help                Show this help message.
  exit                Exit the program.");
    }
}