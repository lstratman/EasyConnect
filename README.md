[![Build status](https://ci.appveyor.com/api/projects/status/u8ugbo8oh1mffhet?svg=true)](https://ci.appveyor.com/project/lstratman/easyconnect)

This is a Windows tabbed remote desktop application whose UI was designed to resemble Chrome's.  Currently it supports Microsoft's Remote Desktop Protocol (RDP), Secure Shell (SSH), Telnet, PowerShell, and VNC but has a plugin architecture designed to enable third-party support for other protocols such as Citrix, etc.

<a href="http://lstratman.github.io/EasyConnect/images/screenshots/bookmarks.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/bookmarks.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/rdp.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/rdp.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/vnc.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/vnc.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/ssh.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/ssh.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/powershell.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/powershell.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/options.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/options.png"/></a>
<a href="http://lstratman.github.io/EasyConnect/images/screenshots/history.png" target="_blank"><img src="http://lstratman.github.io/EasyConnect/images/screenshots/thumbnails/history.png"/></a>

## Installation Options

 - [Release installer](https://lstratman.github.io/EasyConnect/updates/EasyConnect.msi)
 - [CI installer](https://ci.appveyor.com/api/projects/lstratman/EasyConnect/artifacts/EasyConnect.msi)
 - [Microsoft Store](https://www.microsoft.com/en-us/store/p/easyconnect/9nml5g3phdg1?rtc=1)
 - WinGet: `winget install easyconnect`
 - Chocolatey: `choco install easyconnect`

## Intel AMT control for RDP

An RDP host can optionally use Intel AMT control from its settings, below Gateway. Configure its AMT username, password, authentication method (Digest or Kerberos), and TLS. The RDP host name must also resolve to the AMT device, and AMT must be provisioned and reachable directly; the RDP gateway is not used for AMT traffic. Digest requires an AMT username and an 8–32 character password. With Kerberos, credentials can be left empty to use the current identity. TLS requires a trusted server certificate matching the host name. Disabling TLS leaves AMT traffic unencrypted.

When enabled, EasyConnect checks the power state before connecting and sends a power-up request for sleeping, hibernating, or powered-off systems. It waits 10 seconds after the request before starting RDP; the operating system may take longer to start, in which case retry the connection. AMT errors are shown as connection errors. AMT credentials are stored using the same encryption mechanism as other connection passwords.

## VNC display scaling

VNC automatically enables fitting to the window when the remote desktop is wider or taller than the available display area, including after a window or remote resolution change. Choose a display scaling mode from the tools menu to override automatic selection for the current connection. Once enabled, fit remains active until you select original resolution.

## Intel AMT KVM redirection for VNC

VNC connections can optionally use Intel AMT KVM redirection. The AMT target uses port 16994 without TLS or 16995 with TLS; the regular VNC port and display number are ignored in this mode. The existing VNC client handles the RFB session through Intel's local proxy; this does not add RFB 4.0 support. AMT connections use a corrected ZRLE decoder for 16-bit true-color framebuffer data; other VNC connections retain the standard decoder. Authentication is negotiated automatically by Intel's KVM library. Supply an AMT username and password for Digest, or a domain, username, and password for Kerberos. With no AMT credentials, the SDK can use the current Windows identity if the device offers Kerberos. TLS verifies the AMT certificate. The VNC password setting is separate and is not the AMT credential.

The 32-bit Intel `KVMLib.dll` runs in a separate x86 helper process, so the main application remains AnyCPU. The helper requires the Microsoft Visual C++ 2015–2022 x86 runtime. Its local listener is loopback-only but **does not authenticate local clients**: any process running as the same user or another local user can connect to it and may initiate an AMT session using the configured credentials. Use AMT KVM only on trusted computers. TLS protects the AMT network connection, not the helper's local listener. Intel writes a per-session log in a directory restricted to the current Windows user; EasyConnect removes it when the session closes. Forced termination can leave that temporary log behind.

When enabled, EasyConnect checks AMT power status and requests power-up for sleeping, hibernating, or powered-off systems, then waits 10 seconds before starting VNC. If an established AMT VNC session is interrupted, EasyConnect attempts one reconnection per connection session; if it fails or the reconnected session is interrupted, the error is shown. This does not retry or repair incomplete RFB data. AMT errors are reported as connection errors. The Intel SDK binaries are subject to the accompanying Intel AMT SDK license; review its terms and distribution obligations separately from this project's GPL license.

AMT support is enabled at build time when `lib\IntelAMT` contains `HLAPI.dll`, `DotNetWSManClient.dll`, `Intel.Wsman.Scripting.dll`, `IWSManClient.dll`, `WebStorage.dll`, and `Intel AMT SDK License.txt`. VNC AMT KVM additionally requires `KVMLib.dll`; if it is absent, RDP AMT support can still build while VNC AMT KVM is unavailable. Set the MSBuild property `EnableIntelAmt=false` (or `0`/`no`) to disable AMT explicitly, or `EnableIntelAmt=true` (or `1`/`yes`) to require the managed SDK files; the build fails if those files are missing. An unset or blank property enables automatic detection. Pass the same property when building the application and installer. Existing AMT settings are preserved in builds without AMT; a connection still configured to use AMT reports an error until AMT is disabled in its settings or an AMT-enabled build is used.

## Implementing Protocol Plugins

For an example of implementing a protocol plugin, you can look at the `EasyConnect.Protocols.Rdp` project.  You'll want to reference` EasyConnect.Common` and `EasyConnect.Protocols` and then implement classes that inherit from the following base classes:

* `BaseConnection` - This holds the configuration for a connection using your protocol.  Make sure to implement your own `ISerializable` constructor and override `GetObjectData()`.
* `BaseConnectionForm<T>` - This is the form that contains the actual UI controls and logic to create a connection using your protocol.  The easiest thing to do is initially derive from `Form`, design the window (bear in mind that it will be displayed as a child of a `Panel` control with a `BorderStyle` of `None`), and then change the base class to `BaseConnectionForm<T>`.
* `IOptionsForm` - This is the form that will allow the user to configure a connection using your protocol.  Like `BaseConnectionForm<T>`, this will be displayed as a child of a `Panel` control with a `BorderStyle` of `None`.
* `BaseProtocol` - This serves simply to aggregate the previous classes and provide some display data for your new protocol.

That's it!  Just make sure that the assembly for your protocol is in the EasyConnect directory and it will be picked up and used automatically by the application.  The protocol plugin architecture is still evolving, so if you find limitations to the API or want other things added to enable you to implement your protocol, please feel free to contact me or send me a pull request.  I'm also happy to accept pull requests for your protocol projects to include them in the main application.

This project is licensed under the [GPL](GPL.txt)
