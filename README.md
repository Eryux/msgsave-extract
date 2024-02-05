# MS Game - Save Extractor

**MS Game Save Extractor** is a console app to **find**, **list** and **copy** the saves of your Microsoft Store or Game Pass games. The app contain a single executable and should work on any version of **Windows 10** and **Windows 11** with `.NET Runtime 6` installed.

**This app alone doesn't allow to export game save from Microsoft platform to another platform like Steam or Epic etc. For more information about this topic read the answer to the [FAQS: Is it possible to import save in another platform like Steam or Epic ?](#is-it-possible-to-import-save-in-another-platform-like-steam-or-epic-)**.

- **To download and use the app read [Usage](#usage) section**.
- If you encouter **troubleshooting** using the app read the [FAQs](#faqs) first and if you can't find your answer, use the [issues](https://github.com/Eryux/msgsave-extract/issues) section.
- Informations needed **to build** the app are in [Build and development](#build-and-development) section.
- For library usage, see [Using library gpsave-lib in your project](#using-library-gpsave-lib-in-your-project) section and the [library documentation](./docs/gpsave-lib.md).

The solution also include a separated library project containing usefull classes, methods and structs for reading game's save index and containers files if you want to write your own app.


## Usage

- Go to the [Release section](https://github.com/Eryux/msgsave-extract/releases) and download the latest release of the app corresponding to your OS version.
- Unzip the downloaded archive file on your computer.
- Enter the folder obtained by unzipping the archive. You must have two files: `gpsave.exe` and `knowngames.json`.
- Open a terminal in the folder where `gpsave.exe` is located (`Shift+right click > Open PowerShell window here`).
- Run `gpsave.exe [command] [options]` in your terminal with the commands below.

*If the app doesn't start, be sure to have `.NET Runtime 6` installed on your system. Otherwise, you can [download the runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and install it on your system.*


### Commands

List and explanation of availables commands for the app :

#### list-app : List games and apps installed on your system

```
.\gpsave.exe list-app [options]
```
```
.\gpsave.exe list-app

Installed games -

[Stardew Valley] ConcernedApe.StardewValleyPC_0c8vynj4cqe4e
[TUNIC] Finji.TUNIC_tys0ffscxatjj
[Age of Empires Definitive Edition] Microsoft.Darwin_8wekyb3d8bbwe
```

| Option | Description |
| --- | --- |
| --all | Display unknown games or apps. Without this option, only games or apps listed in `knowngames.json` file will be displayed. |


Result in `[]` is the app name on Microsoft Store and the part after is the package name. For unknown app, only the package name will be written.


#### find-saves : List saves of a game or an app

```
.\gpsave.exe find-saves [options]
```
```
.\gpsave.exe find-saves -a "TUNIC"

Find 1 for Finji.TUNIC_tys0ffscxatjj
---
Name: TunicSaves
Created: 09/12/2023 16:35:03.0
Folder: D2A4F50FD3FF46709C9BB77A039D1192
```

| Option | Description |
| --- | --- |
| -a, --app <app-name or package name> | Game or app to list or export saves |


#### export : Copy and export save from a game/app to a folder

```
.\gpsave.exe export [options]
```
```
.\gpsave.exe export -a "TUNIC" -s D2A4F50FD3FF46709C9BB77A039D1192 -o .

2 file(s) copied.

20231209_043503_TunicSaves
|   PLAYERPREFS
|   SAVEDATA
```

| Option | Description |
| --- | --- |
| -a, --app <app-name or package name> | Game or app to list or export saves |
| -s, --save <save> | Folder name of a save to export from game or app |
| -o, --output-dir <path> | Destination folder where exported save will be copied |


#### export-all : Copy and export all save from a game/app to a folder

```
.\gpsave.exe export-all [options]
```
```
.\gpsave.exe export-all -a "Stardew Valley" -o .

4 file(s) copied.

20240105_093250_Doe_364163358
|   SaveData
|   SaveGameInfo
20240118_113808_La_364164461
|   SaveData
|   SaveGameInfo
```

| Option | Description |
| --- | --- |
| -a, --app <app-name or package name> | Game or app to list or export saves |
| -o, --output-dir <path> | Destination folder where exported save will be copied |


#### help : Show help and usage information

```
.\gpsave.exe -h
.\gpsave.exe [command] -h
```
```
Description:
  Find and export your Game Pass saves.

Usage:
  gpsave [command] [options]

Options:
  --nocolor                      Keep default text color in terminal
  -a, --app <app>                Game or app to list or export saves
  -o, --output-dir <output-dir>  Destination folder where exported save will be copied
  --version                      Show version information
  -?, -h, --help                 Show help and usage information

Commands:
  list-app    List games and apps installed on this system
  find-saves  List saves of a game or an app
  export      Copy and export save from a game/app to a folder
  export-all  Copy and export all save from a game/app to a folder
```


### Global options

Global options can be use with all commands.

| Option | Description |
| --- | --- |
| --nocolor | Keep default text color in terminal |
| -?, -h, --help | Show help and usage information |

---

## FAQs

### Will it work if my game is appear unknown in app ?

It might, give it a test. Known games came from Microsoft Game Pass catalog for PC and then saved in `knowngames.json`. So if your game or app is installed outside of Game Pass app (like directly from Microsoft store) or if the game release date is newer that our last update of `knowngames.json` the game will appear unknown in the list but it might be compatible.


### Game is known but is not supported by the app, why ?

Known games are all games in Microsoft Game Pass catalog for PC whether or not they work with the app. It seems that all games featuring **Xbox cloud saves** should be compatible,  others might use their own save methods.


### Is it possible to import save in another platform like Steam or Epic ?

It will really depend of the game. Sometimes you will just need to rename the exported save files with the right name and extension then copy it in the save folder of the game on another platform and it will work without any problem but for other game it will require some extra step or a complete re-writting of the save. So if you want to import your save on another platform, try to find out how save works for your game then, give it a try. If it work or not we will be happy to heard about it ;)


### Can I (re-)import save in a game on Microsoft platform ?

You can re-import an exported save back in your game by overwriting an existing one. To do thisn, follows these steps:
- Use [list-app](#commands) to get the package name of your game then open your file browser,
- Go to `%AppLocalData%\Packages\<package_name>\SystemAppData\wgs` and locate the folder with a file named `containers.index` inside.
- Use the command [find-saves](#commands) to list save folders and their name then open a folder the same "type" as the save you want to import.
- Rename your exported save files to match the name of save file in save folder, you can try to match them by looking at file size or opening the `container.x` file with a hex editor.

Importing a new save into a game is technically possible but it will require to edit the `containers.index` file and create a `container.x` to add a new save entry. And since save files is game dependent we can't program a generic method to automate this process.

---

## Build and development

The project is made with **Visual Studio 2022** and **.NET Core 6**. If you want to build your own version of the app, both are required.

- Clone the repository on your computer then open the solution file (`.sln`) with Visual Studio.

*In the solution you will find two project, `gpsave` and `gpsave-lib`. `gpsave` is the command line tool and `gpsave-lib` is a set of classes and methods to read save file index and container. To build the command line tool you will also need to build the library.*

- Retrieve the NuGet packages required for `gpsave` project then `right click` on the solution and click on `Generate the solution` to build both the command line tool and the library.

- (optional) If you only want a single executable for the command line tool without the `.dll` file then `right click` on the project `gpsave` and select `Publish`. It will open the publishing window, in this window select your architecture target between `win-x86` and `win-x64` then click and `Publish`.

- Finaly copy the file `knowngames.json` next to the command line executable. You can also generate your own `knowgames.json` with the command line tool, see [Update known game catalog](#update-known-game-catalog).

### Using library gpsave-lib in your project

The solution include a project named `gpsave-lib`. Its a .NET Core 6 library containing most of the classes and methods used in the command line tool to read index and container (save) from Microsoft apps. This library can be built and used in other C# project. If you want to know more of how to use the library, check the [documentation](./docs/gpsave-lib.md).

### Update known game catalog

The known games for command line tool are saved in the `knowngames.json` file alongside the executable. This file can be updated or created with the command line tool using the command `update-games`.
```
gpsave.exe update-games
```
This command will use the Xbox website API to retrieve the catalog of Game Pass PC then retrieve the game data for each game in the catalog. To avoid spamming the API a delay of 1.5 seconds is set between each game data request. Generating entierely the `knowngames.json` will take a dozen of minutes and almost 500 HTTP requests.

If the `knowngames.json` file already exists, only game data that are not already present in the file will be fetch through the API.


### Dependencies

- .NET Core 6 - [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- System.CommandLine - [github.com/dotnet/command-line-api](https://github.com/dotnet/command-line-api)
- Json.NET - [www.newtonsoft.com/json](https://www.newtonsoft.com/json)


## Contributing

Contributing is always welcome. 
If you want to help on this project by contributing on its developement, by reporting bugs, errors or mistakes or simply by giving your feedback, use the [issues section](https://github.com/Eryux/msgsave-extract/issues).


## See also

- Good post and r-e work on Game Pass save by [@snoozbuster](https://github.com/snoozbuster): https://github.com/goatfungus/NMSSaveEditor/issues/306.
- A python tool that quite do the same thing but more focused on exporting save from Game Pass to another platform like Steam or Epic: [XGP-save-extractor](https://github.com/Z1ni/XGP-save-extractor)


## License

This project and app are distributed under **MIT License**. See [LICENSE](LICENSE) file for more information.