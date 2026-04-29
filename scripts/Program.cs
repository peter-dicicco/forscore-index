using System.Text.Json;

namespace scripts;

class Program
{
    static void Main()
    {
        string jsonPath =  Path.Combine("../json", "Django Fakebook.json");

        if (!File.Exists(jsonPath))
        {
            Console.Error.WriteLine($"JSON file not found: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        List<Song>? songs = JsonSerializer.Deserialize<List<Song>>(jsonText);

        if (songs is null)
        {
            Console.Error.WriteLine("Failed to deserialize JSON.");
            return;
        }

        List<SongWithEndPage> songsWithEndPage = songs
            .Select((song, index) => new SongWithEndPage(song.Page, song.Title, song.Composer, index < songs.Count - 1 ? songs[index + 1].Page - 1 : song.Page))
            .ToList();

        string outputCsvPath = Path.Combine(Directory.GetCurrentDirectory(), "Django Fakebook.csv");

        using (var writer = new StreamWriter(outputCsvPath))
        {
            //writer.WriteLine("Page,EndPage,Title,Composer");
            foreach (var song in songsWithEndPage)
            {
                writer.WriteLine($"{song.Page},{song.EndPage},{EscapeCsv(song.Title)},{EscapeCsv(song.Composer)}");
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
