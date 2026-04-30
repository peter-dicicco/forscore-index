using System.Text.Json;

namespace scripts;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("This program requires two arguments: the name of the json file in the ../json directory and the display name of the book in ForScore.");
            return;
        }

        string jsonFileName = Path.Combine("../json", args[0]);
        string jsonFileNameWithoutExtension = Path.GetFileNameWithoutExtension(jsonFileName);
        string csvFieName = jsonFileNameWithoutExtension + ".csv";

        if (!File.Exists(jsonFileName))
        {
            Console.Error.WriteLine($"JSON file not found: {jsonFileName}");
            return;
        }

        string jsonText = File.ReadAllText(jsonFileName);
        List<Song>? songs = JsonSerializer.Deserialize<List<Song>>(jsonText);

        if (songs is null)
        {
            Console.Error.WriteLine("Failed to deserialize JSON.");
            return;
        }

        List<SongWithEndPage> songsWithEndPage = songs
            .Select((song, index) => new SongWithEndPage(song.Page, song.Title, song.Composer, index < songs.Count - 1 ? songs[index + 1].Page - 1 : song.Page))
            .ToList();

        string outputCsvPath = Path.Combine("../csv", csvFieName);

        using (var writer = new StreamWriter(outputCsvPath))
        {
            writer.WriteLine("start-page,end-page,title,composer,reference");
            foreach (var song in songsWithEndPage)
            {
                writer.WriteLine($"{song.Page},{song.EndPage},{EscapeCsv(song.Title)},{EscapeCsv(song.Composer)},{EscapeCsv(args[1])}");
            }
        }

        Console.WriteLine($"Wrote {songsWithEndPage.Count} records to '{outputCsvPath}'.");
    }

    static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return '"' + value.Replace("\"", "\"\"") + '"';
        }

        return value;
    }
}

record Song(int Page, string Title, string Composer);
record SongWithEndPage(int Page, string Title, string Composer, int EndPage);
