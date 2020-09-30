[![Build status](https://ci.appveyor.com/api/projects/status/u8ugbo8oh1mffhet?svg=true)](https://ci.appveyor.com/project/lstratman/easyconnect)

This is a Windows tabbed remote desktop application whose UI was designed to resemble Chrome's.  Currently it supports Microsoft's Remote Desktop Protocol (RDP), Secure Shell (SSH), PowerShell, and VNC but has a plugin architecture designed to enable third-party support for other protocols such as Citrix, etc.

<a href="http://lstratman.github.com/EasyConnect/images/screenshots/bookmarks.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/bookmarks.png"/></a>
<a href="http://lstratman.github.com/EasyConnect/images/screenshots/rdp.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/rdp.png"/></a>
<a href="http://lstratman.github.com/EasyConnect/images/screenshots/ssh.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/ssh.png"/></a>
<a href="http://lstratman.github.com/EasyConnect/images/screenshots/powershell.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/powershell.png"/></a>
<a href="http://lstratman.github.com/EasyConnect/images/screenshots/options.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/options.png"/></a>
<a href="http://lstratman.github.com/EasyConnect/images/screenshots/history.png" target="_blank"><img src="http://lstratman.github.com/EasyConnect/images/screenshots/thumbnails/history.png"/></a>

## Installation Options

 - [Release installer](https://lstratman.github.io/EasyConnect/updates/EasyConnect.msi)
 - [CI installer](https://ci.appveyor.com/api/projects/lstratman/EasyConnect/artifacts/EasyConnect.msi)
 - [Microsoft Store](https://www.microsoft.com/en-us/store/p/easyconnect/9nml5g3phdg1?rtc=1)
 - WinGet: `winget install easyconnect`
 - Chocolatey: `choco install easyconnect`

## Implementing Protocol Plugins

For an example of implementing a protocol plugin, you can look at the `EasyConnect.Protocols.Rdp` project.  You'll want to reference` EasyConnect.Common` and `EasyConnect.Protocols` and then implement classes that inherit from the following base classes:

* `BaseConnection` - This holds the configuration for a connection using your protocol.  Make sure to implement your own `ISerializable` constructor and override `GetObjectData()`.
* `BaseConnectionForm<T>` - This is the form that contains the actual UI controls and logic to create a connection using your protocol.  The easiest thing to do is initially derive from `Form`, design the window (bear in mind that it will be displayed as a child of a `Panel` control with a `BorderStyle` of `None`), and then change the base class to `BaseConnectionForm<T>`.
* `IOptionsForm` - This is the form that will allow the user to configure a connection using your protocol.  Like `BaseConnectionForm<T>, this will be displayed as a child of a `Panel` control with a `BorderStyle` of `None`.
* `BaseProtocol` - This serves simply to aggregate the previous classes and provide some display data for your new protocol.

That's it!  Just make sure that the assembly for your protocol is in the EasyConnect directory and it will be picked up and used automatically by the application.  The protocol plugin architecture is still evolving, so if you find limitations to the API or want other things added to enable you to implement your protocol, please feel free to contact me or send me a pull request.  I'm also happy to accept pull requests for your protocol projects to include them in the main application.

This project is licensed under the [GPL](GPL.txt)
