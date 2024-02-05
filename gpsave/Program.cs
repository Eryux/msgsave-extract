using gpsave;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;

// ----------------------------------------------

// Get application version
string appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

// Define text colors
ConsoleColor defaultConsoleColor = Console.ForegroundColor;
ConsoleColor greenConsoleColor = ConsoleColor.Green;
ConsoleColor yellowConsoleColor = ConsoleColor.Yellow;
ConsoleColor redConsoleColor = ConsoleColor.Red;
ConsoleColor whiteConsoleColor = ConsoleColor.White;

// Credits
string credits = $"MS Game - Save File Exporter v{appVersion}\n"
    + "---\n"
    + "Author: Nicolas C. [https://github.com/Eryux]\n"
    + "License: MIT license, this program is distribued free of charge\n"
    + "Github: https://github.com/Eryux/gpsave\n";

// Load known game list
KnownGameCatalog knownGameDirectory;

try {
    knownGameDirectory = KnownGameCatalog.Load("knowngames.json");
} catch {
    knownGameDirectory = new KnownGameCatalog();
}

// ----------------------------------------------

RootCommand rootCmd = new RootCommand("Find and export your Game Pass saves.");

// global: no color option
var nocolorOption = new Option<bool>("--nocolor", "Keep default text color in terminal");
rootCmd.AddGlobalOption(nocolorOption);

// global: --app or -a option
var appOption = new Option<string>("--app", "Game or application to list or export saves");
appOption.AddAlias("-a");
rootCmd.AddGlobalOption(appOption);

// global: --output-dir or -o option
var ouputDirOption = new Option<string>("--output-dir", "Destination folder where exported save will be copied");
ouputDirOption.AddAlias("-o");
rootCmd.AddGlobalOption(ouputDirOption);

// list-app
Command listAppCmd = new Command("list-app", "List games and applications installed on this system");
rootCmd.Add(listAppCmd);

// list-app: --all option
var appAllOption = new Option<bool>("--all", "Display unknown games or applications");
listAppCmd.AddOption(appAllOption);

// find-saves
var findSaveCmd = new Command("find-saves", "List saves of a game or an application");
rootCmd.Add(findSaveCmd);

// export
var exportSaveCmd = new Command("export", "Copy and export save from a game/app to a folder");
rootCmd.Add(exportSaveCmd);

// export: --save or -s
var saveFolderOption = new Option<string>("--save", "Folder name of a save to export from game or application");
saveFolderOption.AddAlias("-s");
exportSaveCmd.AddOption(saveFolderOption);

// export-all
var exportAllSaveCmd = new Command("export-all", "Copy and export all save from a game/app to a folder");
rootCmd.Add(exportAllSaveCmd);

// update-games
var updateGamesCmd = new Command("update-games", "Retrieve game information from MS product catalog and save it to knowngames.json");
updateGamesCmd.IsHidden = true;
rootCmd.Add(updateGamesCmd);

// ----------------------------------------------
// set all text colors to default
// ----------------------------------------------
var commandLineBuilder = new CommandLineBuilder(rootCmd);

commandLineBuilder.AddMiddleware(async (context, next) =>
{
    if (context.ParseResult.GetValueForOption(nocolorOption))
    {
        greenConsoleColor = defaultConsoleColor;
        yellowConsoleColor = defaultConsoleColor;
        redConsoleColor = defaultConsoleColor;
        whiteConsoleColor = defaultConsoleColor;
    }

    await next(context);
});

commandLineBuilder.UseDefaults();

// ----------------------------------------------
// list-app
// ----------------------------------------------
listAppCmd.SetHandler((listAllValue) =>
{
    Dictionary<string, KnownGame> knownGames = knownGameDirectory.Data.ToDictionary(x => x.PackageName, x => x);

    string[] apps = GamePass_SaveReader.ListAppFolder();

    Console.ForegroundColor = whiteConsoleColor;
    Console.WriteLine("Installed games -\n");
    
    for (int i = 0; i < apps.Length; ++i)
    {
        string name = Path.GetFileName(apps[i]);
        if (knownGames.ContainsKey(name))
        {
            Console.ForegroundColor = whiteConsoleColor;
            Console.Write("[");
            Console.ForegroundColor = greenConsoleColor;
            Console.Write(knownGames[name].ProductName);
            Console.ForegroundColor = whiteConsoleColor;
            Console.Write("] ");
            Console.ForegroundColor = yellowConsoleColor;
            Console.WriteLine(name);
        }
    }

    Console.ForegroundColor = defaultConsoleColor;

    if (listAllValue)
    {
        Console.ForegroundColor = whiteConsoleColor;
        Console.WriteLine("\n\nUnknown installed apps -\n");

        for (int i = 0; i < apps.Length; ++i)
        {
            string name = Path.GetFileName(apps[i]);
            if (!knownGames.ContainsKey(name))
            {
                Console.ForegroundColor = yellowConsoleColor;
                Console.WriteLine(name);
            }
        }
    }

}, appAllOption);

// ----------------------------------------------
// find-saves
// ----------------------------------------------
findSaveCmd.SetHandler((appName) =>
{
    KnownGame knownGame = knownGameDirectory.Data.Where(x => x.ProductName == appName).FirstOrDefault();
    string packageName = (knownGame.PackageName == null || knownGame.PackageName.Length == 0) ? appName : knownGame.PackageName;

    if (packageName.Length == 0)
    {
        packageName = appName;
    }

    GamePass_SaveIndex index = null;

    try
    {
        GamePass_SaveReader reader = new GamePass_SaveReader(packageName);
        index = reader.ReadIndex();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = redConsoleColor;
        Console.WriteLine("Game or application isn't supported or save folder doesn't exists.");
        Console.ForegroundColor = defaultConsoleColor;
    }

    if (index != null)
    {
        Console.ForegroundColor = whiteConsoleColor;
        Console.Write("Find ");
        Console.ForegroundColor = greenConsoleColor;
        Console.Write(index.Containers.Length);
        Console.ForegroundColor = whiteConsoleColor;
        Console.Write(" for ");
        Console.ForegroundColor = yellowConsoleColor;
        Console.WriteLine(packageName);
        Console.ForegroundColor = whiteConsoleColor;

        for (int i = 0; i < index.Containers.Length; ++i)
        {
            Console.WriteLine("---");

            Console.ForegroundColor = whiteConsoleColor;
            Console.Write("Name: ");
            Console.ForegroundColor = yellowConsoleColor;
            Console.WriteLine(index.Containers[i].SaveName);

            Console.ForegroundColor = whiteConsoleColor;
            Console.Write("Created: ");
            Console.ForegroundColor = yellowConsoleColor;
            Console.WriteLine(string.Format("{0}.{1}", index.Containers[i].CreatedAt.ToString(), index.Containers[i].CreatedAtPrecision));

            Console.ForegroundColor = whiteConsoleColor;
            Console.Write("Folder: ");
            Console.ForegroundColor = yellowConsoleColor;
            Console.WriteLine(index.Containers[i].FileName);

            Console.ForegroundColor = whiteConsoleColor;
        }
    }

    Console.ForegroundColor = defaultConsoleColor;

}, appOption);

// ----------------------------------------------
// export
// ----------------------------------------------
exportSaveCmd.SetHandler((appName, folderName, outputDir) =>
{
    KnownGame knownGame = knownGameDirectory.Data.Where(x => x.ProductName == appName).FirstOrDefault();
    string packageName = (knownGame.PackageName == null || knownGame.PackageName.Length == 0) ? appName : knownGame.PackageName;

    if (packageName.Length == 0)
    {
        packageName = appName;
    }

    GamePass_SaveReader reader = null;
    GamePass_SaveIndex index = null;

    try
    {
        reader = new GamePass_SaveReader(packageName);
        index = reader.ReadIndex();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = redConsoleColor;
        Console.WriteLine("Game or application isn't supported or save folder doesn't exists.");
        Console.ForegroundColor = defaultConsoleColor;
    }

    if (index != null)
    {
        GamePass_SaveContainer container = null;

        for (int i = 0; i < index.Containers.Length; i++)
        {
            if (index.Containers[i].FileName == folderName)
            {
                container = index.Containers[i];
                break;
            }
        }

        if (container != null)
        {
            try
            {
                int fileSaved = reader.ExportSave(container, outputDir);
                Console.ForegroundColor = greenConsoleColor;
                Console.Write(fileSaved);
                Console.ForegroundColor = whiteConsoleColor;
                Console.WriteLine(" file(s) copied.");
                Console.ForegroundColor = defaultConsoleColor;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = redConsoleColor;
                Console.WriteLine("Unable to export or copy save files, verify your output folder.");
                Console.ForegroundColor = defaultConsoleColor;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = redConsoleColor;
                Console.WriteLine("Unable to export or copy save files, maybe unsupported game or application.");
                Console.ForegroundColor = defaultConsoleColor;
            }
        }
        else
        {
            Console.ForegroundColor = redConsoleColor;
            Console.WriteLine($"Unable to find save folder {folderName} for {appName}");
            Console.ForegroundColor = defaultConsoleColor;
        }
    }
}, appOption, saveFolderOption, ouputDirOption);


// ----------------------------------------------
// export-all
// ----------------------------------------------
exportAllSaveCmd.SetHandler((appName, outputDir) =>
{
    KnownGame knownGame = knownGameDirectory.Data.Where(x => x.ProductName == appName).FirstOrDefault();
    string packageName = (knownGame.PackageName == null || knownGame.PackageName.Length == 0) ? appName : knownGame.PackageName;

    if (packageName.Length == 0)
    {
        packageName = appName;
    }

    GamePass_SaveReader reader = new GamePass_SaveReader(packageName);

    GamePass_SaveIndex index = null;

    try
    {
        index = reader.ReadIndex();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.ForegroundColor = redConsoleColor;
        Console.WriteLine("Game or application isn't supported or save folder doesn't exists.");
        Console.ForegroundColor = defaultConsoleColor;
    }

    if (index != null)
    {
        try
        {
            int fileSaved = reader.ExportSaveAll(index, outputDir);
            Console.ForegroundColor = greenConsoleColor;
            Console.Write(fileSaved);
            Console.ForegroundColor = whiteConsoleColor;
            Console.WriteLine(" file(s) copied.");
            Console.ForegroundColor = defaultConsoleColor;
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = redConsoleColor;
            Console.WriteLine("Unable to export or copy save files, verify your output folder.");
            Console.ForegroundColor = defaultConsoleColor;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = redConsoleColor;
            Console.WriteLine("Unable to export or copy save files, maybe unsupported game or application.");
            Console.ForegroundColor = defaultConsoleColor;
        }
    }
}, appOption, ouputDirOption);

// ----------------------------------------------
// update-games
// ----------------------------------------------
updateGamesCmd.SetHandler(() =>
{
    Task task = Task.Run(async () =>
    {
        Console.ForegroundColor = whiteConsoleColor;
        Console.Write("Retrieve games from MS catalog: ");

        string[] catalog;

        try
        {
            catalog = await KnownGameCatalog.GetCatalog();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = redConsoleColor;
            Console.WriteLine("failed");
            Console.ForegroundColor = defaultConsoleColor;
            Console.WriteLine(ex.Message);
            return;
        }

        Console.ForegroundColor = greenConsoleColor;
        Console.Write(catalog.Length);
        Console.ForegroundColor = whiteConsoleColor;

        Console.WriteLine(" games found");


        Dictionary<string, KnownGame> knownGames = knownGameDirectory.Data.ToDictionary(x => x.ProductId, x => x);

        int cached = 0; int added = 0; int failed = 0;

        for (int i = 0; i < catalog.Length; ++i)
        {
            if (!knownGames.ContainsKey(catalog[i]))
            {
                Console.Write($"Retrieve game {catalog[i]}: ");

                try
                {
                    KnownGame gameInfo = await KnownGameCatalog.GetGame(catalog[i]);
                    await Task.Delay(1500);

                    if (gameInfo.ProductName.Length > 0 && gameInfo.PackageName.Length > 0)
                    {
                        knownGameDirectory.Data.Add(gameInfo);
                        added++;

                        Console.ForegroundColor = whiteConsoleColor;
                        Console.Write("[");
                        Console.ForegroundColor = greenConsoleColor;
                        Console.Write(gameInfo.ProductName);
                        Console.ForegroundColor = whiteConsoleColor;
                        Console.Write("] ");
                        Console.ForegroundColor = yellowConsoleColor;
                        Console.WriteLine(gameInfo.PackageName);
                        Console.ForegroundColor = whiteConsoleColor;
                    }
                    else
                    {
                        failed++;

                        Console.ForegroundColor = redConsoleColor;
                        Console.WriteLine("failed (missing package or product name)");
                        Console.ForegroundColor = defaultConsoleColor;
                    }
                }
                catch (Exception ex)
                {
                    failed++;

                    Console.ForegroundColor = redConsoleColor;
                    Console.WriteLine("failed (error)");
                    Console.ForegroundColor = defaultConsoleColor;
                }
            }
            else
            {
                cached++;
            }
        }

        Console.WriteLine("-");
        Console.Write("Added: ");
        Console.ForegroundColor = greenConsoleColor;
        Console.WriteLine(added);
        Console.ForegroundColor = whiteConsoleColor;
        Console.Write("Already known: ");
        Console.ForegroundColor = yellowConsoleColor;
        Console.WriteLine(cached);
        Console.ForegroundColor = whiteConsoleColor;
        Console.Write("Failed: ");
        Console.ForegroundColor = redConsoleColor;
        Console.WriteLine(failed);
        Console.ForegroundColor = whiteConsoleColor;
        Console.WriteLine("-");
    });

    task.Wait();

    Console.WriteLine("Save game catalog to knowngames.json... ");

    try
    {
        knownGameDirectory.Save(".\\");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = redConsoleColor;
        Console.WriteLine("failed");
        Console.ForegroundColor = defaultConsoleColor;
        Console.WriteLine(ex.Message);
    }
});

// ----------------------------------------------

var parser = commandLineBuilder.Build();

Console.WriteLine(credits);

parser.Invoke(args);

// ----------------------------------------------

Console.ForegroundColor = defaultConsoleColor;