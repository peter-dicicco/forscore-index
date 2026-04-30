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

        List<Song> songsToRemove = [];

        foreach (var song in songs)
        {
            if (song.Title.EndsWith("(1)"))
            {
                //remove the (1) from the title
                song.Title = song.Title.Substring(0, song.Title.Length - 3);
            }
            if (song.Title.EndsWith("(2)") || song.Title.EndsWith("(3)"))
            {
                //add the song to the list of songs to remove
                songsToRemove.Add(song);
            }
        }

        // Remove the songs to remove from the main list
        foreach (var song in songsToRemove)
        {
            songs.Remove(song);
        }

        foreach (var song in songs)
        {
            Console.WriteLine($"Page: {song.Page}, Title: {song.Title}, Composer: {song.Composer}");
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

class Song{
    public int Page {get; set;}
    public string Title  {get; set;} = "";
    public string Composer  {get; set;} = "";
}
record SongWithEndPage(int Page, string Title, string Composer, int EndPage);
