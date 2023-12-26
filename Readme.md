# audiobookshelf-windows
Installs and manages the [audiobookshelf](https://github.com/advplyr/audiobookshelf) server on Windows.

It installs the latest released version of the server itself (pre-packaged as a Windows executable), and a tray app for managing it. 
The tray app runs in the background, and can be accessed by clicking the Audiobookshelf tray icon 
(![Audiobookshelf tray icon](Resources/AppIcon.ico)) in the system tray (bottom right corner of the screen).

The tray app lets you:
- Start/Stop the server
- Set the server to start on login
- View the server logs
- Open the server in your default browser
- Check for updates (upcoming)
- Change the server port and other settings (upcoming)


The tray app was developed in C# using .NET Framework 4.6.1 and Winforms, in Visual Studio 2022 Community Edition. 
It was based on the [audiobookshelf-win](https://github.com/advplyr/audiobookshelf-win) codebase by [advplyr](https://github.com/advplyr).

The installer was developed using [NSIS](https://nsis.sourceforge.io/Main_Page).

## System Requirements
- Windows 10 64-bit or later

You **do not** need to install .NET Framework, as it is included in any Windows 10 installation.

You **do not** need to install Node.js, as the server executable is pre-packaged with it.

## Installation
Download the latest installer release from the [releases page]() and run it.

