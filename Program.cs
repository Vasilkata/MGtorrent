// Standart Libraries
using System;
using System.IO;
using System.Threading.Tasks;

// Libraries From NuGet
using MonoTorrent;
using MonoTorrent.Client;

class Program
{
  public static string downloadsPath = Program.DownloadsFolder;

  static async Task Main(string[] args)
  {
    int argCount = args.Length;
    if(argCount == 0)
    {
      Console.WriteLine("Usage: mgtorrent [torrent]");
      return;
    }
    else if(argCount > 1)
    {
      Console.ForegroundColor = (ConsoleColor)(4);
      Console.WriteLine("Error! Too many arguments!");
      return;
    }

    // Setup paths
    string torrentPath = args[0];

    if(!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);

    // Create the Client Engine
    EngineSettings engineSettings = new EngineSettings();
    ClientEngine engine = new ClientEngine(engineSettings);

    // Load the .torrent file
    Torrent torrent = await Torrent.LoadAsync(torrentPath);
    TorrentManager manager = await engine.AddAsync(torrent, downloadsPath);

    // Start the download
    await manager.StartAsync();

    // Print the name of the torrent in blue
    Console.ForegroundColor = (ConsoleColor)(14);
    Console.Write($"Downloading: ");
    Console.ForegroundColor = (ConsoleColor)(9);
    Console.WriteLine($"{torrent.Name}");

    // Set the color of the loading bar to green
    Console.ForegroundColor = (ConsoleColor)(2);

    // Show progress bar and percentage
    int progressBlocks;
    while(manager.State != TorrentState.Stopped)
    {
      progressBlocks = Convert.ToInt32(manager.Progress / 5);
      Console.Write($"Progress: {manager.Progress:0.00}% ");
      Console.Write($"[{ new string('█', progressBlocks) + new string('░', 20 - progressBlocks) }]\r");

      if(manager.Progress >= 100.0 && manager.State == TorrentState.Seeding)
      {
        Console.WriteLine("Download Complete!" + new string(' ', 22));
        break;
      }

      await Task.Delay(1000);
    }

    // Reset the color back to white
    Console.ForegroundColor = (ConsoleColor)(0);
    await engine.StopAllAsync();
  }

  public static string DownloadsFolder
  {
    get
    {
      // Try the standard User Profile path
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

      // Check if it actually exists
      if(Directory.Exists(path))
        return path;

      //Fallback: Use the desktop or current directory if Downloads is missing
      return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }
  }
}
