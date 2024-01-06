# audiobookshelf-windows
Installs and manages the [audiobookshelf](https://github.com/advplyr/audiobookshelf) server on Windows.

It installs the latest released version of the server itself (pre-packaged as a Windows executable), and a tray app for managing it. 
The tray app runs in the background, and can be accessed by clicking the Audiobookshelf tray icon 
(<img src="Resources/AppIcon.ico" width="16" height="16"/>) in the system tray (bottom right corner of the screen).

The tray app lets you:
- Open the server in your default browser
- Start/Stop the server
- Set the server to start on login
- View the server logs
- Change the server port and other settings
- Check for updates (upcoming)


## System Requirements
- Windows 10 64-bit or later

You **do not** need to install .NET Framework, as it is included in any Windows 10 installation.

You **do not** need to install Node.js, as the server executable is pre-packaged with it.

## Installation
Download the latest installer release from the [release page](https://github.com/mikiher/audiobookshelf-windows/releases/tag/v2.7.1.1) and run it.

## Caveats
- The server currently uses [tone](https://github.com/sandreas/tone) to embed metadata and covers in audio files.
`tone` is slated to be replaced soon, so it is not included in the Windows installer release

## Development
All development was done on a Windows 10 64-bit desktop.

The tray app was developed in C# using .NET Framework 4.6.1 and Winforms.
It was based on the [audiobookshelf-win](https://github.com/advplyr/audiobookshelf-win) codebase by [advplyr](https://github.com/advplyr).

The installer was developed using [Inno Setup](https://jrsoftware.org/isinfo.php).

### 1. Building the Audiobookshelf server executable
- Install [Node.js 16](https://nodejs.org/en/download/) (Must be version 16). 
    - Optional: install [nvm-windows](https://github.com/coreybutler/nvm-windows#installation--upgrades) to manage multiple Node.js versions.
- Install Visual Studio Code
- Clone the [audiobookshelf](https://github.com/advplyr/audiobookshelf.git) Github repository
- Open the `audiobookshelf` folder in Visual Studio Code
- Open the terminal (Ctrl+Shift+`)
- Run `npm ci` to install the dependencies
- Run `npm i pkg -g` to install the pkg (Node.js to executable) package
- Run `npm run build-win` to build the audiobookshelf server executable (it will be placed in the `dist\win` folder)

### 2. Building the Audiobookshelf tray app

The tray can be built using either Visual Studio 2022 or Visual Studio Code.
- If you need to make design changes to the UI, it's recommended to use Visual Studio 2022, as it has a visual designer for Winforms.
- It's convenient to use Visual Studio 2022 since it has a built-in debug console, in which you can see Debeug.WriteLine() messages.
- If you only need to make code changes, you can use Visual Studio Code.

#### On Visual Studio 2022
- Install [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/downloads/)
- Install the [.NET Desktop Development workload](https://learn.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2022#step-4---choose-workloads)
- Clone [this repository](https://github.com/mikiher/audiobookshelf-windows.git)
- Open the `Audiobookshelf.sln` solution file in Visual Studio
- Build the solution (F6) (you will find the executable in the `bin\x64\Release\net461` or `bin\x64\Debug\net461` folder, depending on the build configuration)

#### On Visual Studio Code
- Install [Visual Studio Code](https://code.visualstudio.com/download)
- Install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- Install the latest [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- Clone [this repository](https://github.com/mikiher/audiobookshelf-windows.git)
- Open the terminal (Ctrl+Shift+`)
- Run `dotnet build -c Release` or `dotnet build` or to build the solution (you will find the executable in the `bin\x64\Release\net461` or `bin\x64\Debug\net461` folder, depending on the build configuration)

#### Running the tray app
You can run or debug the tray app directly from Visual Studio 2022 or Visual Studio Code.
- Copy the audiobookshelf server executable to the `bin\x64\Release\net461` or `bin\x64\Debug\net461` folder, depending on the build configuration.
- Run or debug the app (F5). By default, the app will: 
    - run the server on port 13378
    - Will use `%LocalAppData%\Audiobookshelf` as the server data folder.

### 3. Building the installer
- Install [Visual Studio Code](https://code.visualstudio.com/download)
- Install [Inno Setup](https://jrsoftware.org/isinfo.php)
- Install the [Inno Setup extension](https://marketplace.visualstudio.com/items?itemName=Chouzz.vscode-innosetup) for Visual Studio Code
- Clone [this repository](https://github.com/mikiher/audiobookshelf-windows.git)
- Open `Setup\installer.iss` in Visual Studio Code
- Change `#define MyAppBinDir` to the folder where the Audiobookshelf tray app executable and dlls are located
- Change `#define ServerBinDir` to the folder where the Audiobookshelf server executable is located
- Run the `Build Installer` task (Ctrl+Shift+B) to build the installer (you will find it in the `Setup\Output` folder)




